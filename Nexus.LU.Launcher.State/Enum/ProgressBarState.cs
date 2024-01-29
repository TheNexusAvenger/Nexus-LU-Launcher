namespace Nexus.LU.Launcher.State.Enum;

public enum ProgressBarState
{
    Inactive, // No fill.
    PercentFill, // Percent fill.
    Progressing, // Bar is animated but has no specific progress to report.
}