using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Play;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.Gui.Component;

public enum ActiveView
{
    Play,
    Patches,
    Settings,
}

public class ToggleView : Canvas
{
    /// <summary>
    /// View of the server list.
    /// </summary>
    private readonly PlayView playView;
    
    /// <summary>
    /// View of the patches.
    /// </summary>
    // TODO: private readonly PatchesView patchesView;
    
    /// <summary>
    /// View of the settings.
    /// </summary>
    // TODO: private readonly SettingsView settingsView;
    
    /// <summary>
    /// Creates a toggle view panel.
    /// </summary>
    public ToggleView()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.playView = this.Get<PlayView>("PlayView");
        // TODO: this.patchesView = this.Get<PatchesView>("PatchesView");
        // TODO: this.settingsView = this.Get<SettingsView>("SettingsView");
        var playButton = this.Get<ImageTextButton>("PlayButton");
        var patchesButton = this.Get<ImageTextButton>("PatchesButton");
        var settingsButton = this.Get<ImageTextButton>("SettingsButton");

        // Set the active view.
        this.SetView(ActiveView.Play);
        
        // Connect the events.
        var clientState = ClientState.Get();
        this.Get<ImageTextButton>("GitHubButton").ButtonPressed += (sender, args) =>
        {
            // Open the repository in a browser.
            var webProcess = new Process(); 
            webProcess.StartInfo.FileName = "https://github.com/TheNexusAvenger/Nexus-Lego-Universe-Launcher";
            webProcess.StartInfo.UseShellExecute = true;
            webProcess.Start(); 
        };
        playButton.ButtonPressed += (sender, args) =>
        {
            this.SetView(ActiveView.Play);
        };
        patchesButton.ButtonPressed += (sender, args) =>
        {
            this.SetView(ActiveView.Patches);
        };
        settingsButton.ButtonPressed += (sender, args) =>
        {
            this.SetView(ActiveView.Settings);
        };
        clientState.LauncherStateChanged += (state) =>
        {
            this.Run(() =>
            {
                playButton.IsVisible = (state != LauncherState.Launched);
                patchesButton.IsVisible = (state != LauncherState.Launched);
                settingsButton.IsVisible = (state != LauncherState.Launched);
            });
        };
    }
    
    /// <summary>
    /// Sets the view to use.
    /// </summary>
    /// <param name="view">View to use.</param>
    private void SetView(ActiveView view)
    {
        // Update the visibility.
        this.playView.IsVisible = (view == ActiveView.Play);
        // TODO: this.patchesView.IsVisible = (view == ActiveView.Patches);
        // TODO: this.settingsView.IsVisible = (view == ActiveView.Settings);
    }
}