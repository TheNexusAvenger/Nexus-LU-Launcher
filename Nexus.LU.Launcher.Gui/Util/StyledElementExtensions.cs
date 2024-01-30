using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Nexus.LU.Launcher.Gui.Util;

public static class StyledElementExtensions
{
    /// <summary>
    /// Sets a value of an styled element. Unlike the
    /// direct setters, this method is thread-safe.
    /// </summary>
    /// <param name="this">Styled element to set the property of.</param>
    /// <param name="propertyName">Name of the property to set.</param>
    /// <param name="value">Value to set.</param>
    /// <typeparam name="T">Type of the value set.</typeparam>
    public static void SetThreadSafe<T>(this StyledElement @this, string propertyName, T value)
    {
        @this.Run(() =>
        {
            @this.GetType()?.GetProperty(propertyName)?.SetValue(@this, value);
        });
    }
        
    /// <summary>
    /// Calls an action. This method is thread-safe.
    /// </summary>
    /// <param name="this">Styled element to call with.</param>
    /// <param name="action">Action to run.</param>
    public static void Run(this StyledElement @this, Action action)
    {
        Dispatcher.UIThread.InvokeAsync(action);
    }
        
    /// <summary>
    /// Returns the window the styled element is part of, if it exists.
    /// </summary>
    /// <param name="this">Styled element to get the parent window of.</param>
    /// <returns>Parent window that contains the styled element, if it exists.</returns>
    public static Window? GetWindow(this StyledElement @this)
    {
        while (@this != null)
        {
            if (@this is Window window) return window;
            @this = @this.Parent;
        }
        return null;
    }
}