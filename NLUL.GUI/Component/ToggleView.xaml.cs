using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Base;
using NLUL.GUI.Component.Patches;
using NLUL.GUI.Component.Play;
using NLUL.GUI.State;

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
        /// <summary>
        /// View of the server list.
        /// </summary>
        private readonly PlayView playView;
        
        /// <summary>
        /// View of the patches.
        /// </summary>
        private readonly PatchesView patchesView;
        
        /// <summary>
        /// Creates a toggle view panel.
        /// </summary>
        public ToggleView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.playView = this.Get<PlayView>("PlayView");
            this.patchesView = this.Get<PatchesView>("PatchesView");
            var playButton = this.Get<ImageTextButton>("PlayButton");
            var patchesButton = this.Get<ImageTextButton>("PatchesButton");
            var settingsButton = this.Get<ImageTextButton>("SettingsButton");

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
            playButton.ButtonPressed += (sender, args) =>
            {
                this.SetView(ActiveView.Play);
            };
            patchesButton.ButtonPressed += (sender, args) =>
            {
                this.SetView(ActiveView.Patches);
            };
            settingsButton.ButtonPressed += (sender, args) =>
            {
                this.SetView(ActiveView.Settings);
            };
            Client.StateChanged += () =>
            {
                playButton.IsVisible = (Client.State != PlayState.Launched);
                patchesButton.IsVisible = (Client.State != PlayState.Launched);
                settingsButton.IsVisible = (Client.State != PlayState.Launched);
            };
        }
        
        /// <summary>
        /// Sets the view to use.
        /// </summary>
        /// <param name="view">View to use.</param>
        private void SetView(ActiveView view)
        {
            // Update the visibility.
            this.playView.IsVisible = (view == ActiveView.Play);
            this.patchesView.IsVisible = (view == ActiveView.Patches);
        }
    }
}