using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Prompt;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.Gui.Component;

public class TopBar : Panel
{
    /// <summary>
    /// Launcher states that are safe to close without prompting to confirm.
    /// </summary>
    public static readonly List<LauncherState> SafeToCloseStates = new List<LauncherState>()
    {
        LauncherState.Uninitialized,
        LauncherState.ManualRuntimeNotInstalled,
        LauncherState.PendingExtractSelection,
        LauncherState.ExtractFailed,
        LauncherState.VerifyFailed,
        LauncherState.RuntimeNotInstalled,
        LauncherState.NoSelectedServer,
        LauncherState.ReadyToLaunch,
        LauncherState.Launching,
        LauncherState.Launched,
    };
        
    /// <summary>
    /// Creates the top bar.
    /// </summary>
    public TopBar()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);

        // Connect the events.
        this.PointerPressed += (sender,args) => this.GetWindow()?.BeginMoveDrag(args);
        
        // Set up minimizing and closing.
        this.Get<ImageButton>("Minimize").ButtonPressed += (sender, args) =>
        {
            var window = this.GetWindow();
            if (window == null) return;
            window.WindowState = WindowState.Minimized;
        };
        this.Get<ImageButton>("Close").ButtonPressed += (sender, args) =>
        {
            var launcherState = ClientState.Get().CurrentLauncherState;
            if (SafeToCloseStates.Contains(launcherState))
            {
                // Close the window.
                this.GetWindow()?.Close();
                Process.GetCurrentProcess().Kill();
            }
            else
            {
                // Determine the message.
                var message = "Closing the launcher stops extracting the client. Confirm closing?";
                if (launcherState == LauncherState.MovingClient)
                {
                    message = "Closing the launcher stops moving the files. Confirm closing?";
                }

                // Show the prompt.
                ConfirmPrompt.OpenPrompt(message, () =>
                {
                    this.GetWindow()?.Close();
                    Process.GetCurrentProcess().Kill();
                });
            }
        };
        
        // Set the background for accepting events.
        this.Background = new SolidColorBrush(new Color(0, 0, 0, 0));
    }
}