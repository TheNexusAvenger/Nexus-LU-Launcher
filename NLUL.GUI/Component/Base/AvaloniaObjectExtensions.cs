using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace NLUL.GUI.Component.Base
{
    public static class AvaloniaObjectExtensions
    {
        /// <summary>
        /// Sets a value of an Avalonia Object. Unlike the
        /// direct setters, this method is thread-safe.
        /// </summary>
        /// <param name="this">Avalonia Object to set the property of.</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">Value to set.</param>
        /// <typeparam name="T">Type of the value set.</typeparam>
        public static void Set<T>(this AvaloniaObject @this, string propertyName, T value)
        {
            @this.Run(() =>
            {
                @this.GetType()?.GetProperty(propertyName)?.SetValue(@this, value);
            });
        }
        
        /// <summary>
        /// Calls an action. This method is thread-safe.
        /// </summary>
        /// <param name="this">Avalonia Object to call with.</param>
        /// <param name="action">Action to run.</param>
        public static void Run(this AvaloniaObject @this, Action action)
        {
            Dispatcher.UIThread.InvokeAsync(action);
        }
        
        /// <summary>
        /// Returns the window of an Avalonia Object. 
        /// </summary>
        /// <param name="this">Avalonia Object to get the window of.</param>
        /// <returns>The window of the element.</returns>
        public static Window GetWindow(this IControl @this)
        {
            var currentWindow = @this;
            while (currentWindow != null && !(currentWindow is Window))
            {
                currentWindow = currentWindow.Parent;
            }
            return (Window) currentWindow;
        }
    }
}