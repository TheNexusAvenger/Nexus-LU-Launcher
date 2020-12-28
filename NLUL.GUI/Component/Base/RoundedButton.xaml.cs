/*
 * TheNexusAvenger
 *
 * Rounded container for a button.
 */

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace NLUL.GUI.Component.Base
{
    public class RoundedButton : Border
    {
        public static readonly RoutedEvent<RoutedEventArgs> ButtonPressedEvent = RoutedEvent.Register<InputElement,RoutedEventArgs>(nameof(ButtonPressed),RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> ButtonPressed
        {
            add { AddHandler(ButtonPressedEvent,value); }
            remove { RemoveHandler(ButtonPressedEvent,value); }
        }
        
        public IBrush Color
        {
            get { return GetValue(ColorProperty); }
            set { SetValue(ColorProperty,value); }
        }
        public static readonly StyledProperty<IBrush> ColorProperty = AvaloniaProperty.Register<Window,IBrush>(nameof(Color),new SolidColorBrush());
        
        public bool Active
        {
            get { return GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty,value); }
        }
        public static readonly StyledProperty<bool> ActiveProperty = AvaloniaProperty.Register<Window,bool>(nameof(Active),true);
        
        private bool hovering;
        private bool pressing;
        
        /*
         * Creates the rounded button.
         */
        public RoundedButton()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            this.PropertyChanged += (sender,args) =>
            {
                if (args.Property == ColorProperty || args.Property == ActiveProperty)
                {
                    this.UpdateColors();
                }
            };
            this.PointerEnter += (sender,args) =>
            {
                this.hovering = true;
                this.UpdateColors();
            };
            this.PointerLeave += (sender,args) =>
            {
                this.hovering = false;
                this.pressing = false;
                this.UpdateColors();
            };
            this.PointerPressed += (sender,args) =>
            {
                this.pressing = true;
                this.UpdateColors();
                args.Handled = true;
            };
            this.PointerReleased += (sender,args) =>
            {
                if (this.pressing)
                {
                    this.pressing = false;
                    this.UpdateColors();
                    RaiseEvent(new RoutedEventArgs(ButtonPressedEvent));
                }
            };
        }
        
        /*
         * Updates the colors of the button.
         */
        private void UpdateColors()
        {
            // Return if the color is not a solid color brush.
            if (!(this.Color is ISolidColorBrush))
            {
                return;
            }
            
            // Determine the color offset.
            int baseOffset = 0;
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
            var newColor = new Color(color.A, (byte) Math.Clamp(baseOffset + color.R,0,255),(byte) Math.Clamp(baseOffset + color.G,0,255),(byte) Math.Clamp(baseOffset + color.B,0,255));
            this.Background = new SolidColorBrush(newColor);
            this.BorderBrush = new SolidColorBrush(new Color(newColor.A,(byte) (0.8 * newColor.R),(byte) (0.8 * newColor.G),(byte) (0.8 * newColor.B)));
        }
    }
}