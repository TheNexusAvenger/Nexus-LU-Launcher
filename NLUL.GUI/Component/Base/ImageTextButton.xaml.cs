using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace NLUL.GUI.Component.Base
{
    public class ImageTextButton : Panel
    {
        /// <summary>
        /// Event for the button being pressed.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ButtonPressedEvent = RoutedEvent.Register<InputElement, RoutedEventArgs>(nameof(ButtonPressed), RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> ButtonPressed
        {
            add => AddHandler(ButtonPressedEvent, value);
            remove => RemoveHandler(ButtonPressedEvent, value);
        }
        
        /// <summary>
        /// Image of the button.
        /// </summary>
        public string Image
        {
            get => GetValue(ImageProperty);
            set => SetValue(ImageProperty,value);
        }
        public static readonly StyledProperty<string> ImageProperty = AvaloniaProperty.Register<Window, string>(nameof(Image), "");
        
        /// <summary>
        /// Text of the button.
        /// </summary>
        public string Text
        {
            get => GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Window, string>(nameof(Text), "");
        
        /// <summary>
        /// Size of the font of the button.
        /// </summary>
        public double FontSize
        {
            get => GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty,value);
        }
        public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty.Register<Window, double>(nameof(FontSize), 14);
        
        /// <summary>
        /// Image of the button.
        /// </summary>
        private Image buttonImage;
        
        /// <summary>
        /// Text of the button.
        /// </summary>
        private TextBlock buttonText;
        
        /// <summary>
        /// Whether the button is hovering.
        /// </summary>
        private bool hovering;
        
        /// <summary>
        /// Whether the button is pressing.
        /// </summary>
        private bool pressing;
        
        /// <summary>
        /// Creates the image button.
        /// </summary>
        public ImageTextButton()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            this.buttonImage = this.Find<Image>("ButtonImage");
            this.buttonText = this.Find<TextBlock>("ButtonText");

            // Connect the events.
            this.PropertyChanged += (sender,args) =>
            {
                this.UpdateButton();
            };
            this.PointerEnter += (sender,args) =>
            {
                this.hovering = true;
                this.UpdateButton();
            };
            this.PointerLeave += (sender,args) =>
            {
                this.hovering = false;
                this.pressing = false;
                this.UpdateButton();
            };
            this.PointerPressed += (sender,args) =>
            {
                this.pressing = true;
                this.UpdateButton();
                args.Handled = true;
            };
            this.PointerReleased += (sender,args) =>
            {
                if (!this.pressing) return;
                this.pressing = false;
                this.UpdateButton();
                RaiseEvent(new RoutedEventArgs(ButtonPressedEvent));
            };
        }
        
        /// <summary>
        /// Updates the button.
        /// </summary>
        private void UpdateButton()
        {
            // Update the image and color.
            if (this.Image != "")
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                this.buttonImage.Source = new Bitmap(assets.Open(new Uri("avares://" + Assembly.GetEntryAssembly()?.GetName().Name + this.Image)));
            }
            this.buttonImage.Width = this.Height;
            this.buttonImage.Height = this.Height;
            this.buttonText.Text = this.Text;
            this.buttonText.FontSize = this.FontSize;

            // Update the text color.
            if (this.pressing || this.hovering)
            {
                this.buttonText.Foreground = new SolidColorBrush(new Color(255, 255, 255, 255));
            }
            else
            {
                this.buttonText.Foreground = new SolidColorBrush(new Color(255, 127, 127, 127));
            }
        }
    }
}