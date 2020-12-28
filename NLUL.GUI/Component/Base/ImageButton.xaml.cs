/*
 * TheNexusAvenger
 *
 * Button that uses images.
 */

using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace NLUL.GUI.Component.Base
{
    public class ImageButton : Image
    {
        public static readonly RoutedEvent<RoutedEventArgs> ButtonPressedEvent = RoutedEvent.Register<InputElement,RoutedEventArgs>(nameof(PointerEnter),RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> ButtonPressed
        {
            add { AddHandler(ButtonPressedEvent,value); }
            remove { RemoveHandler(ButtonPressedEvent,value); }
        }
        
        public string BaseSource
        {
            get { return GetValue(BaseSourceProperty); }
            set { SetValue(BaseSourceProperty,value); }
        }
        public static readonly StyledProperty<string> BaseSourceProperty = AvaloniaProperty.Register<Window,string>(nameof(BaseSource),"");
        
        public string HoverSource
        {
            get { return GetValue(HoverSourceProperty); }
            set { SetValue(HoverSourceProperty,value); }
        }
        public static readonly StyledProperty<string> HoverSourceProperty = AvaloniaProperty.Register<Window,string>(nameof(HoverSource),"");
        
        public string PressSource
        {
            get { return GetValue(PressSourceProperty); }
            set { SetValue(PressSourceProperty,value); }
        }
        public static readonly StyledProperty<string> PressSourceProperty = AvaloniaProperty.Register<Window,string>(nameof(PressSource),"");
        
        public bool Active
        {
            get { return GetValue(ActiveProperty); }
            set { SetValue(ActiveProperty,value); }
        }
        public static readonly StyledProperty<bool> ActiveProperty = AvaloniaProperty.Register<Window,bool>(nameof(Active),true);

        private bool hovering;
        private bool pressing;
        
        /*
         * Creates the image button.
         */
        public ImageButton()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);

            // Connect the events.
            this.PropertyChanged += (sender,args) =>
            {
                if (args.Property == BaseSourceProperty || args.Property == HoverSourceProperty || args.Property == PressSourceProperty || args.Property == ActiveProperty)
                {
                    this.UpdateSource();
                }
            };
            this.PointerEnter += (sender,args) =>
            {
                this.hovering = true;
                this.UpdateSource();
            };
            this.PointerLeave += (sender,args) =>
            {
                this.hovering = false;
                this.pressing = false;
                this.UpdateSource();
            };
            this.PointerPressed += (sender,args) =>
            {
                this.pressing = true;
                this.UpdateSource();
                args.Handled = true;
            };
            this.PointerReleased += (sender,args) =>
            {
                if (this.pressing)
                {
                    this.pressing = false;
                    this.UpdateSource();
                    RaiseEvent(new RoutedEventArgs(ButtonPressedEvent));
                }
            };
        }
        
        /*
         * Updates the button image.
         */
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
            if (source != "")
            {
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                this.Source = new Bitmap(assets.Open(new Uri("avares://" + Assembly.GetEntryAssembly().GetName().Name + source)));
            }
        }
    }
}