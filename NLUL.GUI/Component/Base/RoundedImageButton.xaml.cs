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
    public class RoundedImageButton : Border
    {
        /// <summary>
        /// Event for the button being pressed.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ButtonPressedEvent = RoutedEvent.Register<InputElement, RoutedEventArgs>(nameof(PointerEnter), RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> ButtonPressed
        {
            add => AddHandler(ButtonPressedEvent, value);
            remove => RemoveHandler(ButtonPressedEvent, value);
        }
        
        /// <summary>
        /// Image of the button when idle.
        /// </summary>
        public string BaseSource
        {
            get => GetValue(BaseSourceProperty);
            set => SetValue(BaseSourceProperty, value);
        }
        public static readonly StyledProperty<string> BaseSourceProperty = AvaloniaProperty.Register<Window, string>(nameof(BaseSource), "");
        
        /// <summary>
        /// Image of the button when hovering.
        /// </summary>
        public string HoverSource
        {
            get => GetValue(HoverSourceProperty);
            set => SetValue(HoverSourceProperty,value);
        }
        public static readonly StyledProperty<string> HoverSourceProperty = AvaloniaProperty.Register<Window, string>(nameof(HoverSource), "");
        
        /// <summary>
        /// Image of the button when pressed.
        /// </summary>
        public string PressSource
        {
            get => GetValue(PressSourceProperty);
            set => SetValue(PressSourceProperty,value);
        }
        public static readonly StyledProperty<string> PressSourceProperty = AvaloniaProperty.Register<Window, string>(nameof(PressSource), "");
        
        /// <summary>
        /// The color of the button.
        /// </summary>
        public IBrush Color
        {
            get => GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }
        public static readonly StyledProperty<IBrush> ColorProperty = AvaloniaProperty.Register<Window, IBrush>(nameof(Color), new SolidColorBrush());
        
        /// <summary>
        /// Whether the button is active.
        /// </summary>
        public bool Active
        {
            get => GetValue(ActiveProperty);
            set => SetValue(ActiveProperty,value);
        }
        public static readonly StyledProperty<bool> ActiveProperty = AvaloniaProperty.Register<Window, bool>(nameof(Active), true);

        /// <summary>
        /// Whether the button is hovered.
        /// </summary>
        private bool hovering;
        
        /// <summary>
        /// Whether the button is pressed.
        /// </summary>
        private bool pressing;
        
        /// <summary>
        /// Creates the rounded image button.
        /// </summary>
        public RoundedImageButton()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            this.PropertyChanged += (sender,args) =>
            {
                if (args.Property == BaseSourceProperty || args.Property == HoverSourceProperty || args.Property == PressSourceProperty || args.Property == ActiveProperty)
                {
                    this.UpdateSource();
                    this.UpdateColors();
                }
            };
            this.PointerEnter += (sender,args) =>
            {
                this.hovering = true;
                this.UpdateSource();
                this.UpdateColors();
            };
            this.PointerLeave += (sender,args) =>
            {
                this.hovering = false;
                this.pressing = false;
                this.UpdateSource();
                this.UpdateColors();
            };
            this.PointerPressed += (sender,args) =>
            {
                this.pressing = true;
                this.UpdateSource();
                this.UpdateColors();
                args.Handled = true;
            };
            this.PointerReleased += (sender,args) =>
            {
                if (!this.pressing) return;
                this.pressing = false;
                this.UpdateSource();
                this.UpdateColors();
                RaiseEvent(new RoutedEventArgs(ButtonPressedEvent));
            };
        }
        
        /// <summary>
        /// Updates the button image.
        /// </summary>
        private void UpdateSource()
        {
            // Determine the button image to use.
            var source = this.BaseSource;
            if (this.Active)
            {
                if (this.pressing)
                {
                    source = this.PressSource;
                }
                else if (this.hovering)
                {
                    source = this.HoverSource;
                }
            }
            
            // Set the image source.
            if (source == "") return;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            this.Get<Image>("Image").Source = new Bitmap(assets.Open(new Uri("avares://" + Assembly.GetEntryAssembly()?.GetName().Name + source)));
        }
        
        /// <summary>
        /// Updates the colors of the button.
        /// </summary>
        private void UpdateColors()
        {
            // Return if the color is not a solid color brush.
            if (!(this.Color is ISolidColorBrush))
            {
                return;
            }
            
            // Determine the color offset.
            var baseOffset = 0;
            if (this.Active)
            {
                if (this.pressing)
                {
                    baseOffset = 30;
                } else if (this.hovering)
                {
                    baseOffset = -30;
                }
            }
            
            // Set the color.
            var color = ((ISolidColorBrush) this.Color).Color;
            var newColor = new Color(color.A, (byte) Math.Clamp(baseOffset + color.R, 0, 255), (byte) Math.Clamp(baseOffset + color.G, 0, 255), (byte) Math.Clamp(baseOffset + color.B, 0, 255));
            this.Background = new SolidColorBrush(newColor);
            this.BorderBrush = new SolidColorBrush(new Color(newColor.A, (byte) (0.8 * newColor.R), (byte) (0.8 * newColor.G), (byte) (0.8 * newColor.B)));
        }
    }
}