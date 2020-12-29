/*
 * TheNexusAvenger
 *
 * Toggles between the play view and host view.
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.Component.Host;
using NLUL.GUI.Component.Play;

namespace NLUL.GUI.Component
{
    public enum ActiveView
    {
        Play,
        Host,
    }
    
    public class ToggleView : Canvas
    {
        private PlayView playView;
        private HostView hostView;
        
        /*
         * Creates a toggle view panel.
         */
        public ToggleView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.playView = this.Get<PlayView>("PlayView");
            this.hostView = this.Get<HostView>("HostView");

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
        }
    }
}