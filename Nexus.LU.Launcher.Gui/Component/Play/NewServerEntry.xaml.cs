using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;

namespace Nexus.LU.Launcher.Gui.Component.Play;

public class NewServerEntry : Border
{
    /// <summary>
    /// Color for the button being enabled.
    /// </summary>
    private static readonly Color ButtonEnabledColor = new Color(255, 0, 120, 205);
    
    /// <summary>
    /// Color for the button being disabled.
    /// </summary>
    private static readonly Color ButtonDisabledColor = new Color(255, 44, 44, 50);
    
    /// <summary>
    /// Stack panel of the inputs.
    /// </summary>
    private readonly StackPanel inputs;
    
    /// <summary>
    /// Input for the server name.
    /// </summary>
    private readonly TextBox serverNameInput;
    
    /// <summary>
    /// Input for the server address.
    /// </summary>
    private readonly TextBox serverAddressInput;
    
    /// <summary>
    /// Button for adding the entry.
    /// </summary>
    private readonly RoundedButton addButton;
    
    /// <summary>
    /// Button for cancelling.
    /// </summary>
    private readonly ImageButton cancelButton;

    /// <summary>
    /// Whether adding is open.
    /// </summary>
    private bool addingOpen = false;
    
    /// <summary>
    /// Creates a server entry creator.
    /// </summary>
    public NewServerEntry()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.inputs = this.Get<StackPanel>("Inputs");
        this.serverNameInput = this.Get<TextBox>("ServerNameInput");
        this.serverAddressInput = this.Get<TextBox>("ServerAddressInput");
        this.addButton = this.Get<RoundedButton>("AddButton");
        this.cancelButton = this.Get<ImageButton>("CancelButton");
        
        // Apply the text.
        var localization = Localization.Get();
        localization.LocalizeText(this.Get<TextBlock>("AddServerNameLabel"));
        localization.LocalizeText(this.Get<TextBlock>("AddAddressLabel"));
        localization.LocalizeText(this.Get<TextBlock>("AddServerButtonText"));

        // Connect the inputs.
        this.addButton.ButtonPressed += (sender, args) =>
        {
            if (this.addingOpen)
            {
                // Add the entry if the inputs are valid.
                if (this.addButton.Active)
                {
                    // Add the entry.
                    ClientState.Get().ServerList.AddEntry(new Nexus.LU.Launcher.State.Model.ServerEntry()
                    {
                        ServerName = this.serverNameInput.Text!.Trim(),
                        ServerAddress = this.serverAddressInput.Text!.Trim(),
                    });

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
    
    /// <summary>
    /// Updates the display.
    /// </summary>
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
            if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(serverAddress) && ClientState.Get().ServerList.GetServerEntry(serverName) == null)
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
    
    /// <summary>
    /// Updates the width.
    /// </summary>
    /// <param name="hasScrollBar">Whether there is a scrollbar.</param>
    public void UpdateWidth(bool hasScrollBar)
    {
        this.MinWidth = hasScrollBar ? 626 : 654;
        this.inputs.MinWidth = hasScrollBar ? 456 : 484;
    }
}