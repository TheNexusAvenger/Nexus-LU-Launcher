/*
 * TheNexusAvenger
 *
 * Buttons displayed at the bottom of the play view.
 */

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Base;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
    public class PlayButtons : DockPanel
    {
        /*
         * Creates the play buttons.
         */
        public PlayButtons()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            var patchesButton = this.Get<ImageTextButton>("PatchesButton");
            this.Get<ImageTextButton>("GitHubButton").ButtonPressed += (sender, args) =>
            {
                // Open the repository in a browser.
                var webProcess = new Process(); 
                webProcess.StartInfo.FileName = "https://github.com/TheNexusAvenger/Nexus-Lego-Universe-Launcher";
                webProcess.StartInfo.UseShellExecute = true;
                webProcess.Start(); 
            };
            this.Get<ImageTextButton>("HostButton").ButtonPressed += (sender, args) =>
            {
                // Get the toggle view.
                IControl toggleView = this;
                while (toggleView != null && !(toggleView is ToggleView))
                {
                    toggleView = toggleView.Parent;
                }
                
                // Toggle the view.
                ((ToggleView) toggleView)?.SetView(ActiveView.Host);
            };
            patchesButton.ButtonPressed += (sender, args) =>
            {
                // Get the toggle view.
                IControl toggleView = this;
                while (toggleView != null && !(toggleView is ToggleView))
                {
                    toggleView = toggleView.Parent;
                }
                
                // Toggle the view.
                ((ToggleView) toggleView)?.SetView(ActiveView.Patches);
            };
            Client.StateChanged += () =>
            {
                // Hide the patches button if the client is not installed.
                var state = Client.state;
                patchesButton.IsVisible = (state == PlayState.NoSelectedServer || state == PlayState.Ready || state == PlayState.Launching);
            };
            patchesButton.IsVisible = (Client.state == PlayState.NoSelectedServer || Client.state == PlayState.Ready || Client.state == PlayState.Launching);
        }
    }
}