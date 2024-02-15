using System;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class LocalArchivePatch : IClientPatch
{
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => false;

    /// <summary>
    /// State of the patch.
    /// </summary>
    public PatchState State => this.ArchivePatch.Installed ? PatchState.Installed : PatchState.NotInstalled;

    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<PatchState>? StateChanged;

    /// <summary>
    /// Archive patch data.
    /// </summary>
    public readonly ArchivePatch ArchivePatch;
    
    /// <summary>
    /// System info of the client.
    /// </summary>
    private readonly SystemInfo systemInfo;

    /// <summary>
    /// Creates the patch.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    /// <param name="archivePatch">Archive patch data to use.</param>
    public LocalArchivePatch(SystemInfo systemInfo, ArchivePatch archivePatch)
    {
        this.systemInfo = systemInfo;
        this.ArchivePatch = archivePatch;
    }

    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public Task RefreshAsync()
    {
        // Nothing to refresh.
        return Task.CompletedTask;
    }

    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        // TODO
    }

    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        // TODO
    }
}