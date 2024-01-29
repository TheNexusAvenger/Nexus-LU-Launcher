namespace Nexus.LU.Launcher.State.Enum;

public enum LauncherState {
    Uninitialized,

    // Manual runtime requirement.
    ManualRuntimeNotInstalled,

    // Client extracting.
    PendingExtractSelection,
    ExtractingClient,
    VerifyingClient,
    ExtractFailed,
    VerifyFailed,

    // Automated runtime.
    RuntimeNotInstalled,
    InstallingRuntime,

    // Ready to play.
    NoSelectedServer,
    ReadyToLaunch,
    Launching,
    Launched,
}