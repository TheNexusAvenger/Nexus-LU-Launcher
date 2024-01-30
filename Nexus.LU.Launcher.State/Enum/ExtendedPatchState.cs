namespace Nexus.LU.Launcher.State.Enum;

public enum ExtendedPatchState
{
    // Normal states.
    Loading,
    Incompatible,
    NotInstalled,
    Installed,
    UpdateAvailable,
    
    // Extended states.
    Installing,
    FailedToInstall,
    Uninstalling,
    FailedToUninstall,
    CheckingForUpdates,
    UpdatesCheckFailed,
    Updating,
    FailedToUpdate,
}