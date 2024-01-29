using System;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.State.Client.Patch;

public interface IClientPatch
{
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name { get; }
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault { get; }
        
    /// <summary>
    /// State of the patch.
    /// </summary>
    public PatchState State { get; }

    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<PatchState> StateChanged;

    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync();
        
    /// <summary>
    /// Installs the patch.
    /// </summary>
    public Task InstallAsync();
        
    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public Task UninstallAsync();
}