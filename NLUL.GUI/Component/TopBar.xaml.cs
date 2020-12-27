/*
 * TheNexusAvenger
 *
 * Top bar of the main window.
 */

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace NLUL.GUI.Component
{
    public class TopBar : Panel
    {
        private Window window;
        
        /*
         * Creates the top bar.
         */
        public TopBar()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            this.AttachedToLogicalTree += (sender,args) => this.RecalculateWindow();
            this.PointerPressed += (sender,args) => window?.BeginMoveDrag(args);
            
            // Set up minimizing and closing.
            this.Get<ImageButton>("Minimize").ButtonPressed += (sender, args) =>
            {
                if (this.window != null)
                {
                    this.window.WindowState = WindowState.Minimized;
                }
            };
            this.Get<ImageButton>("Close").ButtonPressed += (sender, args) => window?.Close();
            
            // Set the background for accepting events.
            this.Background = new SolidColorBrush(new Color(0,0,0,0));
        }
        
        /*
         * Recalculates the parent window.
         */
        private void RecalculateWindow()
        {
            // Move up the tree until a Window is reached or the end is reached.
            IControl currentWindow = this;
            while (currentWindow != null && !(currentWindow is Window))
            {
                currentWindow = currentWindow.Parent;
            }
            
            // Set the window. If no window was found, the window will be null.
            if (currentWindow != null)
            {
                this.window = (Window) currentWindow;
            }
            else
            {
                this.window = null;
            }
        }
    }
}