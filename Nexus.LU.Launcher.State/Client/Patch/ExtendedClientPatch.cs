﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.State.Client.Patch;

public class ExtendedClientPatch
{
    /// <summary>
    /// Lookup for patch states to the extended version.
    /// </summary>
    public readonly Dictionary<PatchState, ExtendedPatchState> PatchStateLookup =
        new Dictionary<PatchState, ExtendedPatchState>()
        {
            {PatchState.Loading, ExtendedPatchState.Loading},
            {PatchState.Incompatible, ExtendedPatchState.Incompatible},
            {PatchState.NotInstalled, ExtendedPatchState.NotInstalled},
            {PatchState.Installed, ExtendedPatchState.Installed},
            {PatchState.UpdateAvailable, ExtendedPatchState.UpdateAvailable},
            {PatchState.UpdatesCheckFailed, ExtendedPatchState.UpdatesCheckFailed},
        };
    
    /// <summary>
    /// Name of the patch.
    /// </summary>
    public string Name => this.clientPatch.Name;
        
    /// <summary>
    /// Description of the patch.
    /// </summary>
    public string Description => this.clientPatch.Description;
    
    /// <summary>
    /// Whether to apply the patch by default.
    /// </summary>
    public bool ApplyByDefault => this.clientPatch.ApplyByDefault;

    /// <summary>
    /// State of the patch.
    /// </summary>
    public ExtendedPatchState State { get; private set; }

    /// <summary>
    /// Event for the state changing.
    /// </summary>
    public event Action<ExtendedPatchState>? StateChanged;
    
    /// <summary>
    /// Patch the extended client patch controls.
    /// </summary>
    private readonly IClientPatch clientPatch;

    /// <summary>
    /// Creates an extended client patch.
    /// </summary>
    /// <param name="clientPatch">Patch to extend.</param>
    public ExtendedClientPatch(IClientPatch clientPatch)
    {
        this.clientPatch = clientPatch;
        this.State = PatchStateLookup[this.clientPatch.State];
        this.clientPatch.StateChanged += (newState) =>
        {
            this.SetState(PatchStateLookup[newState]);
        };
    }

    /// <summary>
    /// Sets the state of the extended client patch.
    /// </summary>
    /// <param name="state">State to set.</param>
    private void SetState(ExtendedPatchState state)
    {
        if (state == this.State) return;
        this.State = state;
        this.StateChanged?.Invoke(state);
    }

    /// <summary>
    /// Refreshes the patch state.
    /// </summary>
    public async Task RefreshAsync()
    {
        try
        {
            this.SetState(ExtendedPatchState.CheckingForUpdates);
            await this.clientPatch.RefreshAsync();
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to refresh {this.clientPatch.Name}.\n{e}");
            this.SetState(ExtendedPatchState.UpdatesCheckFailed);
        }
    }

    /// <summary>
    /// Installs the patch.
    /// </summary>
    public async Task InstallAsync()
    {
        var updating = (this.State == ExtendedPatchState.UpdateAvailable);
        Logger.Debug($"Applying patch {this.clientPatch.Name}.");
        try
        {
            this.SetState(updating ? ExtendedPatchState.Updating : ExtendedPatchState.Installing);
            await this.clientPatch.InstallAsync();
            Logger.Info($"Applied patch {this.clientPatch.Name}.");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to install {this.clientPatch.Name}.\n{e}");
            this.SetState(updating ? ExtendedPatchState.FailedToUpdate : ExtendedPatchState.FailedToInstall);
        }
    }

    /// <summary>
    /// Uninstalls the patch.
    /// </summary>
    public async Task UninstallAsync()
    {
        try
        {
            this.SetState(ExtendedPatchState.Uninstalling);
            await this.clientPatch.UninstallAsync();
            Logger.Info($"Removed patch {this.clientPatch.Name}.");
        }
        catch (Exception e)
        {
            Logger.Error($"Failed to remove {this.clientPatch.Name}.\n{e}");
            this.SetState(ExtendedPatchState.FailedToUninstall);
        }
    }
}