/*
 * TheNexusAvenger
 *
 * Runs the program.
 */

using Avalonia;

namespace NLUL.GUI
{
    class Program
    {
        /*
         * Runs the program.
         */
        public static void Main(string[] args)
        {
            AppBuilder.Configure<App>().UsePlatformDetect().StartWithClassicDesktopLifetime(args);
        }
    }
}