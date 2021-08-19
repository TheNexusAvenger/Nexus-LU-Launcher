using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NLUL.GUI.Component.Base;

namespace NLUL.GUI.Component
{
    public class TopBar : Panel
    {
        /// <summary>
        /// Creates the top bar.
        /// </summary>
        public TopBar()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            this.PointerPressed += (sender,args) => this.GetWindow()?.BeginMoveDrag(args);
            
            // Set up minimizing and closing.
            this.Get<ImageButton>("Minimize").ButtonPressed += (sender, args) =>
            {
                if (this.GetWindow() == null) return;
                this.GetWindow().WindowState = WindowState.Minimized;
            };
            this.Get<ImageButton>("Close").ButtonPressed += (sender, args) =>
            {
                this.GetWindow()?.Close();
                Process.GetCurrentProcess().Kill();
            };
            
            // Set the background for accepting events.
            this.Background = new SolidColorBrush(new Color(0,0,0,0));
        }
    }
}