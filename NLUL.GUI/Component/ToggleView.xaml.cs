/*
 * TheNexusAvenger
 *
 * Toggles between the play view and host view.
 */

using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Base;
using NLUL.GUI.Component.Patches;
using NLUL.GUI.Component.Play;

namespace NLUL.GUI.Component
{
    public enum ActiveView
    {
        Play,
        Patches,
        Settings,
    }
    
    public class ToggleView : Canvas
    {
        private PlayView playView;
        private PatchesView patchesView;
        
        /*
         * Creates a toggle view panel.
         */
        public ToggleView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.playView = this.Get<PlayView>("PlayView");
            this.patchesView = this.Get<PatchesView>("PatchesView");

            // Set the active view.
            this.SetView(ActiveView.Play);
            
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
                this.SetView(ActiveView.Play);
            };
            this.Get<ImageTextButton>("PatchesButton").ButtonPressed += (sender, args) =>
            {
                this.SetView(ActiveView.Patches);
            };
            this.Get<ImageTextButton>("SettingsButton").ButtonPressed += (sender, args) =>
            {
                this.SetView(ActiveView.Settings);
            };
        }
        
        /*
         * Sets the view to use.
         */
        public void SetView(ActiveView view)
        {
            // Update the visibility.
            this.playView.IsVisible = (view == ActiveView.Play);
            this.patchesView.IsVisible = (view == ActiveView.Patches);
        }
    }
}