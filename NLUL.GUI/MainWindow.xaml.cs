/*
 * TheNexusAvenger
 *
 * Main window for Nexus Lego Universe Launcher.
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NLUL.GUI
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Set the window to open at the center.
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}