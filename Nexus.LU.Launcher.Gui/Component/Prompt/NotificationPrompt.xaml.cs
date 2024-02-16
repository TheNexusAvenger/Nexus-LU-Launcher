using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Nexus.LU.Launcher.Gui.Component.Base;

namespace Nexus.LU.Launcher.Gui.Component.Prompt;

public class NotificationPrompt : Window
{
    /// <summary>
    /// Prompt that is currently opened.
    /// </summary>
    public static NotificationPrompt? CurrentPrompt { get; private set; }

    /// <summary>
    /// Whether a prompt is open.
    /// </summary>
    public static bool PromptOpen => (CurrentPrompt != null);
    
    /// <summary>
    /// Event for the prompt being confirmed.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ConfirmedEvent = RoutedEvent.Register<InputElement, RoutedEventArgs>(nameof(PointerEntered), RoutingStrategies.Direct);
    public event EventHandler<RoutedEventArgs> Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }
    
    /// <summary>
    /// Text of the prompt.
    /// </summary>
    public string Text
    {
        get => GetValue(TextProperty);
        set
        {
            SetValue(TextProperty, value);
            this.Get<TextBlock>("Message").Text = value;
        }
    }
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<Window, string>(nameof(Text), "");
    
    /// <summary>
    /// Creates the notification prompt.
    /// </summary>
    public NotificationPrompt()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);

        // Set the window to open at the center.
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        
        // Connect the events.
        this.Get<RoundedImageButton>("Confirm").ButtonPressed += (sender, args) =>
        {
            this.Close();
            CurrentPrompt = null;
            RaiseEvent(new RoutedEventArgs(ConfirmedEvent));
        };
    }

    /// <summary>
    /// Opens a prompt.
    /// </summary>
    /// <param name="message">Message to open.</param>
    /// <param name="confirmed">Callback when the prompt is confirmed.</param>
    public static void OpenPrompt(string message, Action? confirmed = null)
    {
        if (PromptOpen) return;
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            CurrentPrompt = new NotificationPrompt
            {
                Text = message
            };
            if (confirmed != null) { CurrentPrompt.Confirmed += (sender, args) => confirmed(); }
            CurrentPrompt.Show();
        });
    }
}