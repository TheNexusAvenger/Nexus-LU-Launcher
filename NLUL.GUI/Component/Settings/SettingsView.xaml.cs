using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace NLUL.GUI.Component.Settings
{
    public class SettingsView : Panel
    {
        /// <summary>
        /// Creates a settings view.
        /// </summary>
        public SettingsView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

        }
    }
}