using Avalonia;
using Avalonia.Controls;

namespace Nexus.LU.Launcher.Gui.Util;

public static class StyledElementExtensions
{ 
    /// <summary>
    /// Returns the window the styled element is part of, if it exists.
    /// </summary>
    /// <param name="this">Styled element to get the parent window of.</param>
    /// <returns>Parent window that contains the styled element, if it exists.</returns>
    public static Window? GetWindow(this StyledElement? @this)
    {
        while (@this != null)
        {
            if (@this is Window window) return window;
            @this = @this.Parent;
        }
        return null;
    }
}