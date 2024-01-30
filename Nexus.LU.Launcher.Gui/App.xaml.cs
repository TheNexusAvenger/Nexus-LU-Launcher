namespace Nexus.LU.Launcher.Gui;

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

public class App : Application
{
    /// <summary>
    /// Initializes the app.
    /// </summary>
    public override void Initialize()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
    }
    
    /// <summary>
    /// Invoked when the initialization of the app is complete.
    /// </summary>
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