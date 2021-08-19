using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using NLUL.GUI.Component.Base;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Play
{
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
            set => SetValue(SelectedProperty,value);
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
                PersistentState.SetSelectedServer(this.ServerName);
            };
            removeButton.ButtonPressed += (sender, args) =>
            {
                PersistentState.RemoveServerEntry(this.ServerName);
            };
            PersistentState.SelectedServerChanged += this.UpdateSelectButton;
            
            // Update the initial button.
            this.UpdateSelectButton();
        }
        
        /// <summary>
        /// Updates the select button.
        /// </summary>
        private void UpdateSelectButton()
        {
            if (this.Selected)
            {
                this.selectButton.Color = new SolidColorBrush(new Color(0, 0, 0, 0));
                this.selectText.Text = "Selected";
            }
            else
            {
                this.selectButton.Color = new SolidColorBrush(new Color(255, 0, 120, 205));
                this.selectText.Text = "Select";
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
}