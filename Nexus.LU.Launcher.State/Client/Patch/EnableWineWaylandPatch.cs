using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class EnableWineWaylandPatch : IClientPatch
{
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault { get; private set; } = false;

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
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public EnableWineWaylandPatch(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        this.RefreshAsync();
    }
    
    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        // Set the state as incompatible if Wayland is active or WINE isn't installed.
        var newState = PatchState.NotInstalled;
        if (Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")?.ToLower() != "wayland")
        {
            newState = PatchState.Incompatible;
        }
        if (Environment.GetEnvironmentVariable("PATH")!.Split(":").All(directory => !File.Exists(Path.Combine(directory, "wine"))))
        {
            newState = PatchState.Incompatible;
        }
        
        // Set the state based on if the Wayland state is enabled.
        if (newState != PatchState.Incompatible && this.systemInfo.GetPatchStore("EnableWineWayland", "ForceWaylandDriver")?.ToLower() == "true")
        {
            newState = PatchState.Installed;
        }
        
        // Set the state.
        this.ApplyByDefault = (newState != PatchState.Incompatible);
        this.State = newState;
        this.StateChanged?.Invoke(this.State);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        // Enable the Wayland driver.
        Logger.Debug("Enabling Wayland driver in registry.");
        await this.RunRegCommand("reg.exe add HKCU\\Software\\Wine\\Drivers /v Graphics /d wayland,x11 /f");
        
        // Set forcing Wayland as enabled.
        this.systemInfo.SetPatchStore("EnableWineWayland", "ForceWaylandDriver", "true");
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }
        
    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        // Disable the Wayland driver.
        Logger.Debug("Removing Wayland driver in registry.");
        await this.RunRegCommand("reg.exe delete HKCU\\Software\\Wine\\Drivers /v Graphics /f");
        
        // Set forcing Wayland as not enabled.
        this.systemInfo.SetPatchStore("EnableWineWayland", "ForceWaylandDriver", null);
        this.systemInfo.SaveSettings();
        await this.RefreshAsync();
    }

    /// <summary>
    /// Runs a Registry command.
    /// </summary>
    /// <param name="command">Command to run.</param>
    private async Task RunRegCommand(string command)
    {
        var regProcess = new Process
        {
            StartInfo =
            {
                FileName = "wine",
                Arguments = command,
                CreateNoWindow = true,
            }
        };
        regProcess.StartInfo.EnvironmentVariables["WINEPREFIX"] = Path.Combine(this.systemInfo.ClientLocation, "..", "WinePrefix");
        regProcess.Start();
        await regProcess.WaitForExitAsync();
    }
}