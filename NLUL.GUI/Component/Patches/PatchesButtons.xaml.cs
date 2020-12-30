/*
 * TheNexusAvenger
 *
 * Buttons displayed at the bottom of the patches view.
 */

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Base;

namespace NLUL.GUI.Component.Patches
{
    public class PatchesButtons : DockPanel
    {
        /*
         * Creates a patches buttons.
         */
        public PatchesButtons()
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
            this.Get<ImageTextButton>("PlayButton").ButtonPressed += (sender, args) =>
            {
                // Get the toggle view.
                IControl toggleView = this;
                while (toggleView != null && !(toggleView is ToggleView))
                {
                    toggleView = toggleView.Parent;
                }
                
                // Toggle the view.
                ((ToggleView) toggleView)?.SetView(ActiveView.Play);
            };
        }
    }
}