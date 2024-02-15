using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Prompt;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;

namespace Nexus.LU.Launcher.Gui.Component.Play;

public class ServerEntry : Border
{
    /// <summary>
    /// The name of the server.
    /// </summary>
    public string ServerName
    {
        get => GetValue(ServerNameProperty);
        set => SetValue(ServerNameProperty, value);
    }
    public static readonly StyledProperty<string> ServerNameProperty = AvaloniaProperty.Register<Window, string>(nameof(ServerName), "");
    
    /// <summary>
    /// The address of the server.
    /// </summary>
    public string ServerAddress
    {
        get => GetValue(ServerAddressProperty);
        set => SetValue(ServerAddressProperty, value);
    }
    public static readonly StyledProperty<string> ServerAddressProperty = AvaloniaProperty.Register<Window, string>(nameof(ServerAddress), "");
    
    /// <summary>
    /// Whether the server entry is selected.
    /// </summary>
    public bool Selected
    {
        get => GetValue(SelectedProperty);
        set => SetValue(SelectedProperty, value);
    }
    public static readonly StyledProperty<bool> SelectedProperty = AvaloniaProperty.Register<Window, bool>(nameof(Selected), false);

    /// <summary>
    /// Background of the server entry.
    /// </summary>
    private readonly StackPanel backgroundPanel;
    
    /// <summary>
    /// Select button of the server entry.
    /// </summary>
    private readonly RoundedButton selectButton;
    
    /// <summary>
    /// Select text of the server entry.
    /// </summary>
    private readonly TextBlock selectText;
    
    /// <summary>
    /// Creates a server entry.
    /// </summary>
    public ServerEntry()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.backgroundPanel = this.Get<StackPanel>("Background");
        this.selectButton = this.Get<RoundedButton>("SelectButton");
        this.selectText = this.Get<TextBlock>("SelectText");
        var serverNameText = this.Get<TextBlock>("ServerName");
        var serverAddressText = this.Get<TextBlock>("ServerAddress");
        var removeButton = this.Get<ImageButton>("RemoveButton");
        
        // Connect the events.
        var clientState = ClientState.Get();
        this.PropertyChanged += (sender, args) =>
        {
            if (args.Property == SelectedProperty)
            {
                // Update the selected button.
                this.UpdateSelectButton();
            }
            else if (args.Property == ServerNameProperty)
            {
                // Update the server name.
                serverNameText.Text = this.ServerName;
            }
            else if (args.Property == ServerAddressProperty)
            {
                // Update the server address.
                serverAddressText.Text = this.ServerAddress;
            }
        };
        this.selectButton.ButtonPressed += (sender, args) =>
        {
            clientState.ServerList.SetServerActive(this.ServerName);
        };
        removeButton.ButtonPressed += (sender, args) =>
        {
            ConfirmPrompt.OpenPrompt(string.Format(Localization.Get().GetLocalizedString("ServerMenu_ConfirmRemovingPrompt"), this.ServerName), () =>
            {
                clientState.ServerList.RemoveEntry(this.ServerName);
            });
        };
        clientState.ServerList.ServerListChanged += () =>
        {
            this.RunMainThread(this.UpdateSelectButton);
        };
        
        // Update the initial button.
        this.UpdateSelectButton();
    }
    
    /// <summary>
    /// Updates the select button.
    /// </summary>
    private void UpdateSelectButton()
    {
        var localization = Localization.Get();
        if (this.Selected)
        {
            this.selectButton.Color = new SolidColorBrush(new Color(0, 0, 0, 0));
            this.selectText.Text = localization.GetLocalizedString("ServerMenu_SelectedLabel");
        }
        else
        {
            this.selectButton.Color = new SolidColorBrush(new Color(255, 0, 120, 205));
            this.selectText.Text = localization.GetLocalizedString("ServerMenu_SelectButtonText");
        }
    }
    
    /// <summary>
    /// Updates the width.
    /// </summary>
    /// <param name="hasScrollBar">Whether there is a scroll bar.</param>
    public void UpdateWidth(bool hasScrollBar)
    {
        this.backgroundPanel.MinWidth = hasScrollBar ? 456 : 484;
    }
}