/*
 * TheNexusAvenger
 *
 * View for the play screen.
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NLUL.GUI.Component.Play
{
    public class PlayView : Panel
    {
        /*
        * Creates a play view.
        */
        public PlayView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
        }
    }
}