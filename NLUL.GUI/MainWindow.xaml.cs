using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NLUL.GUI
{
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
}