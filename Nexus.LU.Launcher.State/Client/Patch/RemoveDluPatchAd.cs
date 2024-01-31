using System;
using System.IO;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class RemoveDluPatchAd : IClientPatch
{
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name => "Remove DLU Ad";
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description => "Removes the advertisement for DLU from the zone loading screen. This is to return the localization file back to the original version.";

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
    /// Location of the locale file.
    /// </summary>
    private string LocaleFileLocation => Path.Combine(systemInfo.ClientLocation, "locale", "locale.xml");
    
    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public RemoveDluPatchAd(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
        Task.Run(this.RefreshAsync);
    }
    
    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public async Task RefreshAsync()
    {
        if (!File.Exists(this.LocaleFileLocation))
        {
            this.State = PatchState.Incompatible;
        }
        else
        {
            // To keep it hidden, the patch needs to be considered incompatible after installed.
            this.State = (await File.ReadAllTextAsync(this.LocaleFileLocation)).Contains("DLU is coming!") ? PatchState.NotInstalled : PatchState.Incompatible;
        }
        this.StateChanged?.Invoke(this.State);
    }
    
    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        if (File.Exists(LocaleFileLocation))
        {
            await File.WriteAllTextAsync(this.LocaleFileLocation,
                (await File.ReadAllTextAsync(this.LocaleFileLocation))
                    .Replace("DLU is coming!", "Build on Nimbus Isle!")
                    .Replace("Follow us on Twitter", "Get inspired and build on Nimbus Station&apos;s largest Property!")
                    .Replace("@darkflameuniv", "Look for the launch pad by the water&apos;s edge in Brick Annexe!"));
        }
        await this.RefreshAsync();
    }
    
    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public Task UninstallAsync()
    {
        // Uninstalling this patch is not supported.
        return Task.CompletedTask;
    }
}