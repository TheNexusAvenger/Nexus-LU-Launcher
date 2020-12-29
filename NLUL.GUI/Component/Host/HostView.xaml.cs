/*
 * TheNexusAvenger
 *
 * View for the host screen.
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NLUL.GUI.Component.Host
{
    public class HostView : StackPanel
    {
        /*
         * Creates a host view.
         */
        public HostView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
        }
    }
}