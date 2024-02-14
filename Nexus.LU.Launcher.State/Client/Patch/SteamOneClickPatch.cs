using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nexus.LU.Launcher.State.Client.Patch.Valve;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class SteamOneClickPatch : IPreLaunchClientPatch
{
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => false;
    
    /// <summary>
    /// State of the patch.
    /// </summary>
    public PatchState State { get; private set;  } = PatchState.Loading;
    
    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<PatchState>? StateChanged;
    
    /// <summary>
    /// System info of the client.
    /// </summary>
    private readonly SystemInfo systemInfo;

    /// <summary>
    /// Location of Steam for the system.
    /// </summary>
    private string? steamLocation;

    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public SteamOneClickPatch(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        this.RefreshAsync();
    }

    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        // Get the location of Steam.
        this.steamLocation = null;
        var potentialSteamPaths = new List<string>()
        {
            @"C:\Program Files (x86)\Steam",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "Steam"), // Arch Linux (Steam Deck)
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".steam", "debian-installation"), // Debian
        };
        this.steamLocation = potentialSteamPaths.FirstOrDefault(path => Directory.Exists(path));
        
        // Set the patch as unsupported if Steam is not found.
        if (this.steamLocation == null)
        {
            this.State = PatchState.Incompatible;
            return Task.CompletedTask;
        }
        
        // The setup for Steam is not undone, and may be done by the user already.
        // Because of this, the state is a simple stored value that may be incorrect.
        this.State = (this.systemInfo.GetPatchStore("SteamOneClick", "State") == null
            ? PatchState.NotInstalled
            : PatchState.Installed);
        this.StateChanged?.Invoke(this.State);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        // Add the launcher as a non-Steam application.
        var executable = Environment.ProcessPath;
        var startDirectory = Environment.CurrentDirectory;
        var launchOptions = "";
        var flatpakId = Environment.GetEnvironmentVariable("FLATPAK_ID");
        if (flatpakId != null)
        {
            executable = "flatpak";
            launchOptions = $"\"run\" \"{flatpakId}\"";
        }

        var entryAdded = false;
        foreach (var userPath in Directory.GetDirectories(Path.Combine(this.steamLocation!, "userdata")))
        {
            // Ignore the shortcuts file if Nexus LU Launcher already exists.
            var shortcutsPath = Path.Combine(userPath, "config", "shortcuts.vdf");
            ValveDataFormatShortcutsFile shortcutsFile;
            if (File.Exists(shortcutsPath))
            {
                shortcutsFile = ValveDataFormatShortcutsFile.FromFile(shortcutsPath);
            }
            else
            {
                shortcutsFile = new ValveDataFormatShortcutsFile()
                {
                    Name = "shortcuts",
                };
            }
            if (shortcutsFile.Values.Select(pair => pair.Value)
                .FirstOrDefault(entry => entry.TryGetHeader("AppName") == "Nexus LU Launcher") != null) continue;
            
            // Add the shortcut and save the file.
            Logger.Info($"Adding shortcut to {shortcutsPath}");
            shortcutsFile.AddEntry(new ValveDataFormatEntryList()
            {
                new TextValveDataFormatEntry("appid", new byte[] {64, 201, 67, 159}),
                new HeaderValveDataFormatEntry("AppName", "Nexus LU Launcher"),
                new HeaderValveDataFormatEntry("Exe", $"\"{executable}\""),
                new HeaderValveDataFormatEntry("StartDir", $"\"{startDirectory}\""),
                new HeaderValveDataFormatEntry("icon", ""), // TODO: Add icon.
                new HeaderValveDataFormatEntry("ShortcutPath", ""),
                new HeaderValveDataFormatEntry("LaunchOptions", launchOptions),
                new TextValveDataFormatEntry("IsHidden"),
                new TextValveDataFormatEntry("AllowDesktopConfig", new byte[] {1, 0, 0, 0}),
                new TextValveDataFormatEntry("AllowOverlay", new byte[] {1, 0, 0, 0}),
                new TextValveDataFormatEntry("OpenVR"),
                new TextValveDataFormatEntry("Devkit"),
                new HeaderValveDataFormatEntry("DevkitGameID", ""),
                new TextValveDataFormatEntry("DevkitOverrideAppId"),
                new TextValveDataFormatEntry("LastPlayTime"),
                new SetValveDataFormatEntry<string>("tags"),
            });
            shortcutsFile.Write(shortcutsPath);
            entryAdded = true;
        }
        
        // Shut down Steam if an entry was added.
        // The call to add the controller config will restart Steam.
        if (entryAdded)
        {

            if (flatpakId == null)
            {
                // Stop the Steam process.
                Logger.Info("Shutting down Steam.");
                foreach (var steamProcess in Process.GetProcessesByName("steam"))
                {
                    Logger.Debug($"Stopping process id {steamProcess.Id}.");
                    steamProcess.Kill();
                    await steamProcess.WaitForExitAsync();
                }
            }
            else
            {
                // Store the last time the log had "Shutdown".
                // It is assumed that if the line is different, then a new shutdown was performed by the user.
                var bootstrapLogPath = Path.Combine(this.steamLocation!, "logs", "bootstrap_log.txt");
                if (File.Exists(bootstrapLogPath))
                {
                    var lastShutdownLine = "";
                    var bootstrapLogLines = await File.ReadAllLinesAsync(bootstrapLogPath);
                    if (bootstrapLogLines.Length >= 1 && bootstrapLogLines[^1].Contains("Shutdown"))
                    {
                        Logger.Info("Steam is not active and does not need to be restarted.");
                    }
                    else
                    {
                        Logger.Warn("Unable to restart Steam from a Flatpak. A manual restart will be required.");
                        foreach (var line in bootstrapLogLines)
                        {
                            if (!line.Contains("Shutdown")) continue;
                            lastShutdownLine = line;
                        }

                        this.systemInfo.SetPatchStore("SteamOneClick", "LastSteamShutdownLine", lastShutdownLine);
                        this.systemInfo.SaveSettings();
                    }
                }
            }
        }
        
        // Prompt setting the controller layout.
        await this.PromptControllerLayoutAsync();
        
        // Set the patch as installed and ready for the settings change.
        if (this.systemInfo.GetPatchStore("SteamOneClick", "State") == null)
        {
            this.systemInfo.SetPatchStore("SteamOneClick", "State", "PendingSettingsChange");
            this.systemInfo.SaveSettings();
        }
        await this.RefreshAsync();
    }

    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        this.systemInfo.SetPatchStore("SteamOneClick", "State", null);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }

    /// <summary>
    /// Performs operations between setting the boot.cfg and launching
    /// the client. This will yield launching the client.
    /// </summary>
    public async Task OnClientRequestLaunchAsync()
    {
        // Prompt to set the controller layout.
        if (this.systemInfo.GetPatchStore("SteamOneClick", "LastSteamShutdownLine") != null)
        {
            await this.PromptControllerLayoutAsync();
        }
        
        // Return if the patch is not active.
        if (this.systemInfo.GetPatchStore("SteamOneClick", "State") != "PendingSettingsChange")
        {
            Logger.Debug("Settings file not changed because the state was not set to do so.");
            return;
        }

        // Find the LEGO Universe settings file.
        var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LEGO Software", "LEGO Universe", "lwo.xml");
        var winePrefixUsersPath = Path.Combine(this.systemInfo.SystemFileLocation, "WinePrefix", "drive_c", "users");
        if (Directory.Exists(winePrefixUsersPath)) {
            foreach (var winePrefixUserPath in Directory.GetDirectories(winePrefixUsersPath))
            {
                var winePrefixSettingsPath = Path.Combine(winePrefixUserPath, "AppData", "Local", "LEGO Software", "LEGO Universe", "lwo.xml");
                if (!File.Exists(winePrefixSettingsPath)) continue;
                settingsPath = winePrefixSettingsPath;
            }
        }
        
        // Return if the settings file does not exist.
        if (!File.Exists(settingsPath))
        {
            Logger.Warn("Settings file not found. This is normal if this is the first launch of the game.");
            return;
        }
        
        try
        {
            // Change the settings.
            var settingsDocument = XDocument.Load(settingsPath);
            foreach (var element in settingsDocument.Descendants("ConfigurableOption"))
            {
                var nameAttribute = element.Attribute("name");
                if (nameAttribute == null) continue;
                if (nameAttribute.Value != "WINDOWED" && nameAttribute.Value != "WINDOW_MAXIMIZED") continue;
                var valueElement = element.Descendants("Value").FirstOrDefault();
                if (valueElement == null) continue;
                valueElement.Value = "1"; // For WINDOWED, this makes it not full screen. For WINDOW_MAXIMIZED, this makes it maximized.
            }

            // Save the settings.
            settingsDocument.Save(settingsPath);
            Logger.Info("Updated settings to not be in full screen.");
            
            // Set the state as completed.
            this.systemInfo.SetPatchStore("SteamOneClick", "State", "SettingsChanged");
            this.systemInfo.SaveSettings();
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to update client settings.\n{e}");
        }
    }

    /// <summary>
    /// Prompts to set the controller layout.
    /// </summary>
    private async Task PromptControllerLayoutAsync()
    {
        // Return if the last shutdown line is the same.
        var previousLastShutdownTime = this.systemInfo.GetPatchStore("SteamOneClick", "LastSteamShutdownLine");
        if (previousLastShutdownTime != null)
        {
            var lastShutdownLine = "";
            var bootstrapLogPath = Path.Combine(this.steamLocation!, "logs", "bootstrap_log.txt");
            foreach (var line in await File.ReadAllLinesAsync(bootstrapLogPath))
            {
                if (!line.Contains("Shutdown")) continue;
                lastShutdownLine = line;
            }
            if (lastShutdownLine == previousLastShutdownTime)
            {
                Logger.Warn("Controller layout prompt is pending, but a restart of Steam was not detected.");
                return;
            }
            this.systemInfo.SetPatchStore("SteamOneClick", "LastSteamShutdownLine", null);
            this.systemInfo.SaveSettings();
        }
        
        // Prompt to change the layout.
        Logger.Debug("Prompting for controller layout.");
        var webProcess = new Process(); 
        webProcess.StartInfo.FileName = "steam://controllerconfig/2672019776/3160129134";
        webProcess.StartInfo.UseShellExecute = true;
        webProcess.Start();
    }
}