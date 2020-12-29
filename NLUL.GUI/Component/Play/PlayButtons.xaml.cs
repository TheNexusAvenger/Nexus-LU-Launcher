/*
 * TheNexusAvenger
 *
 * Buttons displayed at the bottom of the play view.
 */

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Base;

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
                // TODO: Implement
            };
            this.Get<ImageTextButton>("PatchesButton").ButtonPressed += (sender, args) =>
            {
                // TODO: Implement
            };
        }
    }
}