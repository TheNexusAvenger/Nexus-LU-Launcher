using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using NLUL.GUI.Component.Base;

namespace NLUL.GUI.Component.Prompt
{
    public class ConfirmPrompt : Window
    {
        /// <summary>
        /// Prompt that is currently opened.
        /// </summary>
        public static ConfirmPrompt CurrentPrompt { get; private set; }

        /// <summary>
        /// Whether a prompt is open.
        /// </summary>
        public static bool PromptOpen => (CurrentPrompt != null);
        
        /// <summary>
        /// Event for the prompt being confirmed.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> ConfirmedEvent = RoutedEvent.Register<InputElement, RoutedEventArgs>(nameof(PointerEnter), RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> Confirmed
        {
            add => AddHandler(ConfirmedEvent, value);
            remove => RemoveHandler(ConfirmedEvent, value);
        }
        
        /// <summary>
        /// Event for the prompt being cancelled.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> CancelledEvent = RoutedEvent.Register<InputElement, RoutedEventArgs>(nameof(PointerEnter), RoutingStrategies.Direct);
        public event EventHandler<RoutedEventArgs> Cancelled
        {
            add => AddHandler(CancelledEvent, value);
            remove => RemoveHandler(CancelledEvent, value);
        }
        
        /// <summary>
        /// Text of the button.
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
        /// Creates the confirm prompt.
        /// </summary>
        public ConfirmPrompt()
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
            this.Get<RoundedImageButton>("Cancel").ButtonPressed += (sender, args) =>
            {
                this.Close();
                CurrentPrompt = null;
                RaiseEvent(new RoutedEventArgs(CancelledEvent));
            };
        }

        /// <summary>
        /// Opens a prompt.
        /// </summary>
        /// <param name="message">Message to open.</param>
        /// <param name="confirmed">Callback when the prompt is confirmed.</param>
        /// <param name="cancelled">Callback when the prompt is cancelled.</param>
        public static void OpenPrompt(string message, Action confirmed = null, Action cancelled = null)
        {
            if (PromptOpen) return;
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentPrompt = new ConfirmPrompt
                {
                    Text = message
                };
                if (confirmed != null) { CurrentPrompt.Confirmed += (sender, args) => confirmed(); }
                if (cancelled != null) { CurrentPrompt.Cancelled += (sender, args) => cancelled(); }
                CurrentPrompt.Show();
            });
        }
    }
}