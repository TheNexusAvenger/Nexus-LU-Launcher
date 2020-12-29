/*
 * TheNexusAvenger
 *
 * App for Nexus Lego Universe Launcher.
 */

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NLUL.GUI.State;

namespace NLUL.GUI
{
    public class App : Application
    {
        /*
         * Initializes the app.
         */
        public override void Initialize()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            
            // Load the launcher state.
            PersistentState.LoadState();
        }
        
        /*
         * Invoked when the initialization of the app is complete.
         */
        public override void OnFrameworkInitializationCompleted()
        {
            // Create the main window.
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            // Call the original method.
            base.OnFrameworkInitializationCompleted();
        }
    }
}