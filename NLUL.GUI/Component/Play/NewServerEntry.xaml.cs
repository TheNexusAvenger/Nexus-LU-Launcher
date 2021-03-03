/*
 * TheNexusAvenger
 *
 * Adds entries to the server list.
 */

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NLUL.GUI.Component.Base;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
    public class NewServerEntry : Border
    {
        public static readonly Color ButtonEnabledColor = new Color(255,0,120,205);
        public static readonly Color ButtonDisabledColor = new Color(255,44,44,50);
            
        private StackPanel inputs;
        private TextBox serverNameInput;
        private TextBox serverAddressInput;
        private RoundedButton addButton;
        private ImageButton cancelButton;

        private bool addingOpen = false;
        
        /*
        * Creates a server entry creator.
        */
        public NewServerEntry()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.inputs = this.Get<StackPanel>("Inputs");
            this.serverNameInput = this.Get<TextBox>("ServerNameInput");
            this.serverAddressInput = this.Get<TextBox>("ServerAddressInput");
            this.addButton = this.Get<RoundedButton>("AddButton");
            this.cancelButton = this.Get<ImageButton>("CancelButton");

            // Connect the inputs.
            this.addButton.ButtonPressed += (sender, args) =>
            {
                if (this.addingOpen)
                {
                    // Add the entry if the inputs are valid.
                    if (this.addButton.Active)
                    {
                        // Add the entry.
                        PersistentState.AddServerEntry(this.serverNameInput.Text.Trim(),
                            this.serverAddressInput.Text.Trim());

                        // Clear the inputs.
                        this.serverNameInput.Text = "";
                        this.serverAddressInput.Text = "";

                        // Close adding.
                        this.addingOpen = false;
                        this.Update();
                    }
                }
                else
                {
                    // Open adding.
                    this.addingOpen = true;
                    this.Update();
                }
            };
            this.cancelButton.ButtonPressed += (sender, args) =>
            {
                // Clear the inputs.
                this.serverNameInput.Text = "";
                this.serverAddressInput.Text = "";

                // Close adding.
                this.addingOpen = false;
                this.Update();
            };
            this.serverNameInput.PropertyChanged += (sender, args) =>
            {
                
                this.Update();
            };
            this.serverAddressInput.PropertyChanged += (sender, args) =>
            {
                
                this.Update();
            };

            // Update the initial display.
            this.Update();
        }
        
        /*
         * Updates the display.
         */
        private void Update()
        {
            if (this.addingOpen)
            {
                // Enable the inputs.
                this.inputs.IsVisible = true;
                this.cancelButton.IsVisible = true;
                
                // Update the button depending on if the inputs are populated and the name isn't a duplicate.
                var serverName = this.serverNameInput.Text?.Trim();
                var serverAddress = this.serverAddressInput.Text?.Trim();
                if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(serverAddress) && PersistentState.GetServerEntry(serverName) == null)
                {
                    this.addButton.Active = true;
                    this.addButton.Color = new SolidColorBrush(ButtonEnabledColor);
                }
                else
                {
                    this.addButton.Active = false;
                    this.addButton.Color = new SolidColorBrush(ButtonDisabledColor);
                }
            }
            else
            {
                // Hide the inputs and enable the button.
                this.inputs.IsVisible = false;
                this.cancelButton.IsVisible = false;
                this.addButton.Active = true;
                this.addButton.Color = new SolidColorBrush(ButtonEnabledColor);
            }
        }
        
        /*
         * Updates the width.
         */
        public void UpdateWidth(bool hasScrollBar)
        {
            this.MinWidth = hasScrollBar ? 626 : 654;
            this.inputs.MinWidth = hasScrollBar ? 456 : 484;
        }
    }
}