using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using InfectedRose.Core;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class AutoTcpUdpPatch : IPreLaunchClientPatch
{
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name => "Auto TCP/UDP Shim";
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description => "Enables connecting to community-run LEGO Universe servers that may or may not use TCP/UDP. This is automatically managed for the requested server. Requires the Mod Loader to be installed. Can't be installed with TCP/UDP Shim.";
    
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => false;

    /// <summary>
    /// State of the patch.
    /// </summary>
    public PatchState State { get; private set; } = PatchState.Loading;

    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<PatchState>? StateChanged;
    
    /// <summary>
    /// System info of the client.
    /// </summary>
    private readonly SystemInfo systemInfo;

    /// <summary>
    /// Location of the mod folder.
    /// </summary>
    private string ModFolderLocation => Path.Join(this.systemInfo.ClientLocation, "mods", "raknet_replacer");
    
    /// <summary>
    /// Location of the mod.
    /// </summary>
    private string ModLocation => Path.Join(ModFolderLocation, "mod.dll");

    /// <summary>
    /// Location of the mod folder when disabled.
    /// </summary>
    private string DisabledModFolderLocation => Path.Join(this.systemInfo.ClientLocation, "disabledmods", "raknet_replacer");
    
    /// <summary>
    /// Location of the mod when disabled.
    /// </summary>
    private string DisabledModLocation => Path.Join(DisabledModFolderLocation, "mod.dll");

    /// <summary>
    /// Latest tag of the mod loaders in GitHub.
    /// </summary>
    private string? latestTag;

    /// <summary>
    /// Whether the update check failed.
    /// </summary>
    private bool latestTagCheckFailed = false;
    
    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public AutoTcpUdpPatch(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        Task.Run(async () =>
        {
            try
            {
                this.latestTag = await GitHubUtil.GetLatestTagAsync("lcdr/raknet_shim_dll");
            }
            catch (Exception)
            {
                this.latestTagCheckFailed = true;
            }
            await this.RefreshAsync();
        });
    }
    
    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        if (this.latestTag == null && !this.latestTagCheckFailed)
        {
            this.State = PatchState.Loading;
        }
        else if ((!File.Exists(this.ModLocation) || this.systemInfo.GetPatchStore("TcpUdp", "InstalledVersion") != null) && !File.Exists(this.DisabledModLocation))
        {
            this.State = PatchState.NotInstalled;
        }
        else if (this.latestTagCheckFailed)
        {
            this.State = PatchState.UpdatesCheckFailed;
        }
        else if (this.latestTag != this.systemInfo.GetPatchStore("AutoTcpUdp", "InstalledVersion"))
        {
            this.State = PatchState.UpdateAvailable;
        }
        else
        {
            this.State = PatchState.Installed;
        }
        this.StateChanged?.Invoke(this.State);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        // Throw an exception if there is no latest tag.
        if (this.latestTag == null)
        {
            throw new InvalidOperationException("Latest tag does not exist.");
        }
        
        // Throw an exception if TcpUdp is installed.
        if (this.systemInfo.GetPatchStore("TcpUdp", "InstalledVersion") != null)
        {
            throw new InvalidOperationException("TcpUdp already installed.");
        }
        
        // Create the mod directory.
        if (!Directory.Exists(this.ModFolderLocation))
        {
            Directory.CreateDirectory(this.ModFolderLocation);
        }
        
        // Remove the existing mod.dll.
        if (File.Exists(this.ModLocation))
        {
            File.Delete(this.ModLocation);
        }
        
        // Download the mod.
        var client = new HttpClient();
        await client.DownloadFileAsync("https://github.com/lcdr/raknet_shim_dll/releases/download/" + this.latestTag + "/mod.dll", this.ModLocation);

        // Save installed version.
        this.systemInfo.SetPatchStore("AutoTcpUdp", "InstalledVersion", this.latestTag);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }

    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        // Remove the mod directory.
        if (Directory.Exists(this.ModFolderLocation))
        {
            Directory.Delete(this.ModFolderLocation, true);
        }
        if (Directory.Exists(this.ModFolderLocation))
        {
            Directory.Delete(this.DisabledModFolderLocation, true);
        }
        
        // Save that no version is installed.
        this.systemInfo.SetPatchStore("AutoTcpUdp", "InstalledVersion", null);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }
        
    /// <summary>
    /// Performs and operations between setting the boot.cfg and launching
    /// the client. This will yield launching the client.
    /// </summary>
    public async Task OnClientRequestLaunchAsync()
    {
        // Determine the host to check.
        var bootConfig = LegoDataDictionary.FromString((await File.ReadAllTextAsync(Path.Combine(this.systemInfo.ClientLocation, "boot.cfg").Replace("\n", ""))).Trim(), ',');
        var host = (string) bootConfig["AUTHSERVERIP"];
        Logger.Info($"Check for TCP/UDP for: {host}");
        
        // Assume TCP/UDP if any port is specified.
        // Even if 1001, the stock client will not connect correctly.
        if (host.Contains(":"))
        {
            var portString = host.Remove(0, host.IndexOf(":", StringComparison.Ordinal) + 1).Trim();
            if (int.TryParse(portString, out var port))
            {
                Logger.Info($"Custom port {port} specified. Assuming TCP/UDP.");
                this.SwitchModDirectory(this.DisabledModFolderLocation, this.ModFolderLocation);
                return;
            }
        }
        
        // Try to connect and disconnect from port 21836 (default TCP/UDP port).
        // Port 1001 is more likely to be used by other applications like games.
        try
        {
            // Enable TCP/UDP after a successful connect and close.
            var client = new TcpClient(host, 21836);
            client.Close();
            Logger.Info("Connection to default TCP/UDP port 21836 successful. Assuming TCP/UDP.");
            this.SwitchModDirectory(this.DisabledModFolderLocation, this.ModFolderLocation);
        }
        catch (Exception)
        {
            // Disable TCP/UDP (assume RakNet).
            Logger.Info("Connection to default TCP/UDP port 21836 failed. Assuming not TCP/UDP.");
            this.SwitchModDirectory(this.ModFolderLocation, this.DisabledModFolderLocation);
        }
    }
    
    /// <summary>
    /// Switches the directories of the mod.
    /// </summary>
    /// <param name="source">Source of the mod file.</param>
    /// <param name="target">New target of the mod file.</param>
    private void SwitchModDirectory(string source, string target)
    {
        var sourceMod = Path.Combine(source, "mod.dll");
        if (!Directory.Exists(target))
        {
            Directory.CreateDirectory(target);
        }
        if (File.Exists(sourceMod))
        {
            File.Move(sourceMod, Path.Combine(target, "mod.dll"));
        }
        if (Directory.Exists(source))
        {
            Directory.Delete(source, true);
        }
    }
}