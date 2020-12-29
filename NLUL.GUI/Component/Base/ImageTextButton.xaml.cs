/*
 * TheNexusAvenger
 *
 * Button that contains an image and text.
 */

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
        public static readonly RoutedEvent<RoutedEventArgs> ButtonPressedEvent = RoutedEvent.Register<InputElement,RoutedEventArgs>(nameof(ButtonPressed),RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> ButtonPressed
        {
            add { AddHandler(ButtonPressedEvent,value); }
            remove { RemoveHandler(ButtonPressedEvent,value); }
        }
        
        public string Image
        {
            get { return GetValue(ImageProperty); }
            set { SetValue(ImageProperty,value); }
        }
        public static readonly StyledProperty<string> ImageProperty = AvaloniaProperty.Register<Window,string>(nameof(Image),"");
        
        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty,value); }
        }
        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Window,string>(nameof(Text),"");
        
        public double FontSize
        {
            get { return GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty,value); }
        }
        public static readonly StyledProperty<double> FontSizeProperty = AvaloniaProperty.Register<Window,double>(nameof(FontSize),14);
        
        private Image buttonImage;
        private TextBlock buttonText;
        private bool hovering;
        private bool pressing;
        
        /*
         * Creates the image button.
         */
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
                if (this.pressing)
                {
                    this.pressing = false;
                    this.UpdateButton();
                    RaiseEvent(new RoutedEventArgs(ButtonPressedEvent));
                }
            };
        }
        
        /*
         * Updates the button.
         */
        public void UpdateButton()
        {
            // Update the image and color.
            if (this.Image != "")
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                this.buttonImage.Source = new Bitmap(assets.Open(new Uri("avares://" + Assembly.GetEntryAssembly().GetName().Name + this.Image)));
            }
            this.buttonImage.Width = this.Height;
            this.buttonImage.Height = this.Height;
            this.buttonText.Text = this.Text;
            this.buttonText.FontSize = this.FontSize;

            // Update the text color.
            if (this.pressing || this.hovering)
            {
                this.buttonText.Foreground = new SolidColorBrush(new Color(255,255,255,255));
            }
            else
            {
                this.buttonText.Foreground = new SolidColorBrush(new Color(255,127,127,127));
            }
        }
    }
}