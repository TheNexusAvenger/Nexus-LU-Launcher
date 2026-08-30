using Avalonia;
using Nexus.LU.Launcher.State.Client.Patch;

namespace Nexus.LU.Launcher.Gui;

public class Program
{
    /// <summary>
    /// Runs the program.
    /// </summary>
    /// <param name="args">Arguments from the command line.</param>
    public static void Main(string[] args)
    {
        var builder = AppBuilder.Configure<App>().UsePlatformDetect();
        if (EnableWineWaylandPatch.CanUseWayland())
        {
            builder.UseWayland();
        }
        builder.StartWithClassicDesktopLifetime(args);
    }
}