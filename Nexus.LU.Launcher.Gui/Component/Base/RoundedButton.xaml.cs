using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Nexus.LU.Launcher.Gui.Component.Base;

public class RoundedButton : Border
{
    /// <summary>
    /// Event for the button being pressed.
    /// </summary>
    public event EventHandler<RoutedEventArgs> ButtonPressed
    {
        add => AddHandler(ButtonPressedEvent, value);
        remove => RemoveHandler(ButtonPressedEvent, value);
    }
    public static readonly RoutedEvent<RoutedEventArgs> ButtonPressedEvent = RoutedEvent.Register<InputElement, RoutedEventArgs>(nameof(ButtonPressed), RoutingStrategies.Direct);
    
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
    /// Whether the button is enabled.
    /// </summary>
    public bool Active
    {
        get => GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }
    public static readonly StyledProperty<bool> ActiveProperty = AvaloniaProperty.Register<Window, bool>(nameof(Active), true);
    
    /// <summary>
    /// Whether the button is being hovered.
    /// </summary>
    private bool hovering;
    
    /// <summary>
    /// Whether the button is pressing.
    /// </summary>
    private bool pressing;
    
    /// <summary>
    /// Creates the rounded button.
    /// </summary>
    public RoundedButton()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);

        // Connect the events.
        this.PropertyChanged += (sender, args) =>
        {
            if (args.Property != ColorProperty && args.Property != ActiveProperty) return;
            this.UpdateColors();
        };
        this.PointerEntered += (sender,args) =>
        {
            this.hovering = true;
            this.UpdateColors();
        };
        this.PointerExited += (sender, args) =>
        {
            this.hovering = false;
            this.pressing = false;
            this.UpdateColors();
        };
        this.PointerPressed += (sender, args) =>
        {
            this.pressing = true;
            this.UpdateColors();
            args.Handled = true;
        };
        this.PointerReleased += (sender, args) =>
        {
            if (!this.pressing) return;
            this.pressing = false;
            this.UpdateColors();
            RaiseEvent(new RoutedEventArgs(ButtonPressedEvent));
        };
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