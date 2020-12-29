/*
 * TheNexusAvenger
 *
 * Selectable server entry.
 */

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
        public string ServerName
        {
            get { return GetValue(ServerNameProperty); }
            set { SetValue(ServerNameProperty,value); }
        }
        public static readonly StyledProperty<string> ServerNameProperty = AvaloniaProperty.Register<Window,string>(nameof(ServerName),"");
        
        public string ServerAddress
        {
            get { return GetValue(ServerAddressProperty); }
            set { SetValue(ServerAddressProperty,value); }
        }
        public static readonly StyledProperty<string> ServerAddressProperty = AvaloniaProperty.Register<Window,string>(nameof(ServerAddress),"");
        
        public bool Selected
        {
            get { return GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty,value); }
        }
        public static readonly StyledProperty<bool> SelectedProperty = AvaloniaProperty.Register<Window,bool>(nameof(Selected),false);

        private TextBlock serverNameText;
        private TextBlock serverAddressText;
        private RoundedButton selectButton;
        private TextBlock selectText;
        private ImageButton removeButton;
        
        /*
        * Creates a server entry.
        */
        public ServerEntry()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.serverNameText = this.Get<TextBlock>("ServerName");
            this.serverAddressText = this.Get<TextBlock>("ServerAddress");
            this.selectButton = this.Get<RoundedButton>("SelectButton");
            this.selectText = this.Get<TextBlock>("SelectText");
            this.removeButton = this.Get<ImageButton>("RemoveButton");
            
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
                    this.serverNameText.Text = this.ServerName;
                }
                else if (args.Property == ServerAddressProperty)
                {
                    // Update the server address.
                    this.serverAddressText.Text = this.ServerAddress;
                }
            };
            this.selectButton.ButtonPressed += (sender, args) =>
            {
                PersistentState.SetSelectedServer(this.ServerName);
            };
            this.removeButton.ButtonPressed += (sender, args) =>
            {
                PersistentState.RemoveServerEntry(this.ServerName);
            };
            PersistentState.SelectedServerChanged += this.UpdateSelectButton;
            
            // Update the initial button.
            this.UpdateSelectButton();
        }
        
        /*
         * Updates the select button.
         */
        private void UpdateSelectButton()
        {
            if (this.Selected)
            {
                this.selectButton.Color = new SolidColorBrush(new Color(0,0,0,0));
                this.selectText.Text = "Selected";
            }
            else
            {
                this.selectButton.Color = new SolidColorBrush(new Color(255,0,120,205));
                this.selectText.Text = "Select";
            }
        }
    }
}