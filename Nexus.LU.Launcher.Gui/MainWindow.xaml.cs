using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Nexus.LU.Launcher.Gui;

public class MainWindow : Window
{
    /// <summary>
    /// Creates the main window.
    /// </summary>
    public MainWindow()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);

        // Set the window to open at the center.
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}