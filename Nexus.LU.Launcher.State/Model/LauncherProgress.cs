using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.State.Model;

public class LauncherProgress
{
    /// <summary>
    /// Current state of the launcher.
    /// </summary>
    public LauncherState LauncherState { get; set; }

    /// <summary>
    /// Current state of the progress bar.
    /// </summary>
    public ProgressBarState ProgressBarState { get; set; } = ProgressBarState.Inactive;
    
    /// <summary>
    /// Percentage to fill the progress bar if ProgressBarState is PercentFill.
    /// </summary>
    public double? ProgressBarFill { get; set; }
    
    /// <summary>
    /// Additional string for the current progress, typically when an error occurs.
    /// </summary>
    public string? AdditionalData { get; set; }
}