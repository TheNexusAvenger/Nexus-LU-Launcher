using System;
using System.IO;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class FixAvantGardensSurvivalCrashPatch : IClientPatch
{
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name => "Fix Avant Gardens Survival Crash";
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description => "Fixes a mistake in the Avant Gardens Survival script that results in players crashing in Avant Gardens Survival if they are not the first player.";

    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => true;

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
    /// Location of the Avant Gardens Survival client file.
    /// </summary>
    private string SurvivalScriptFileLocation => Path.Combine(systemInfo.ClientLocation, "res", "scripts", "ai", "minigame", "survival", "l_zone_survival_client.lua");
    
    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public FixAvantGardensSurvivalCrashPatch(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        this.RefreshAsync();
    }

    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        if (!File.Exists(this.SurvivalScriptFileLocation))
        {
            this.State = PatchState.Incompatible;
        }
        else
        {
            this.State = File.ReadAllText(this.SurvivalScriptFileLocation).Contains("    PlayerReady(self)") ? PatchState.NotInstalled : PatchState.Installed;
        }
        this.StateChanged?.Invoke(this.State);
        return Task.CompletedTask;
    }
        
    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        if (!File.Exists(this.SurvivalScriptFileLocation)) return;
        await File.WriteAllTextAsync(this.SurvivalScriptFileLocation,
            (await File.ReadAllTextAsync(this.SurvivalScriptFileLocation))
                .Replace("    PlayerReady(self)", "    onPlayerReady(self)"));
        await this.RefreshAsync();
    }
        
    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        if (!File.Exists(this.SurvivalScriptFileLocation)) return;
        await File.WriteAllTextAsync(this.SurvivalScriptFileLocation,
            (await File.ReadAllTextAsync(this.SurvivalScriptFileLocation))
                .Replace("    onPlayerReady(self)", "    PlayerReady(self)"));
        this.State = PatchState.NotInstalled;
        this.StateChanged?.Invoke(this.State);
        await this.RefreshAsync();
    }
}