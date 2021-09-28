using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NLUL.GUI.Component.Base;
using NLUL.GUI.Component.Prompt;
using NLUL.GUI.State;

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
                if (Client.State.SafeToClose)
                {
                    // Close the window.
                    this.GetWindow()?.Close();
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    // Determine the message.
                    var message = "Closing the launcher stops downloading the client. Confirm closing?";
                    if (Client.State == PlayState.MovingClientDirectory)
                    {
                        message = "Closing the launcher stops moving the files. Confirm closing?";
                    }
                    
                    // Show the prompt.
                    ConfirmPrompt.OpenPrompt(message, () =>
                    {
                        this.GetWindow()?.Close();
                        Process.GetCurrentProcess().Kill();
                    });   
                }
            };
            
            // Set the background for accepting events.
            this.Background = new SolidColorBrush(new Color(0,0,0,0));
        }
    }
}