/*
 * TheNexusAvenger
 *
 * Toggles between the play view and host view.
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Host;
using NLUL.GUI.Component.Patches;
using NLUL.GUI.Component.Play;

namespace NLUL.GUI.Component
{
    public enum ActiveView
    {
        Play,
        Host,
        Patches,
    }
    
    public class ToggleView : Canvas
    {
        private PlayView playView;
        private HostView hostView;
        private PatchesView patchesView;
        
        /*
         * Creates a toggle view panel.
         */
        public ToggleView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.playView = this.Get<PlayView>("PlayView");
            this.hostView = this.Get<HostView>("HostView");
            this.patchesView = this.Get<PatchesView>("PatchesView");

            // Set the active view.
            this.SetView(ActiveView.Play);
        }
        
        /*
         * Sets the view to use.
         */
        public void SetView(ActiveView view)
        {
            // Update the visibility.
            this.playView.IsVisible = (view == ActiveView.Play);
            this.hostView.IsVisible = (view == ActiveView.Host);
            this.patchesView.IsVisible = (view == ActiveView.Patches);
        }
    }
}