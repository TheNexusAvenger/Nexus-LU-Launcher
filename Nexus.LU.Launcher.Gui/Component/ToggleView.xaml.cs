using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nexus.LU.Launcher.Gui.Component.Base;
using Nexus.LU.Launcher.Gui.Component.Patches;
using Nexus.LU.Launcher.Gui.Component.Play;
using Nexus.LU.Launcher.Gui.Component.Settings;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.Gui.Component;

public class ToggleView : Canvas
{
    /// <summary>
    /// Panels that can be toggled.
    /// </summary>
    private readonly List<Panel> panels;
    
    /// <summary>
    /// Creates a toggle view panel.
    /// </summary>
    public ToggleView()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.panels = new List<Panel>()
        {
            this.Get<PlayView>("PlayView"),
            this.Get<PatchesView>("PatchesView"),
            this.Get<SettingsView>("SettingsView"),
        };
        var gitHubButton = this.Get<ImageTextButton>("GitHubButton");
        var playButton = this.Get<ImageTextButton>("PlayButton");
        var patchesButton = this.Get<ImageTextButton>("PatchesButton");
        var settingsButton = this.Get<ImageTextButton>("SettingsButton");
        
        // Apply the text.
        var localization = Localization.Get();
        localization.LocalizeText(gitHubButton);
        localization.LocalizeText(playButton);
        localization.LocalizeText(patchesButton);
        localization.LocalizeText(settingsButton);

        // Set the active view.
        this.SetView("PlayView");
        
        // Connect the events.
        var clientState = ClientState.Get();
        gitHubButton.ButtonPressed += (sender, args) =>
        {
            // Open the repository in a browser.
            var webProcess = new Process(); 
            webProcess.StartInfo.FileName = "https://github.com/TheNexusAvenger/Nexus-Lego-Universe-Launcher";
            webProcess.StartInfo.UseShellExecute = true;
            webProcess.Start(); 
        };
        playButton.ButtonPressed += (sender, args) =>
        {
            this.SetView("PlayView");
        };
        patchesButton.ButtonPressed += (sender, args) =>
        {
            this.SetView("PatchesView");
        };
        settingsButton.ButtonPressed += (sender, args) =>
        {
            this.SetView("SettingsView");
        };
        clientState.LauncherStateChanged += (state) =>
        {
            this.RunMainThread(() =>
            {
                playButton.IsVisible = (state != LauncherState.Launched);
                patchesButton.IsVisible = (state != LauncherState.Launched);
                settingsButton.IsVisible = (state != LauncherState.Launched);
                if (state == LauncherState.Launched)
                {
                    this.SetView("PlayView");
                }
            });
        };
    }
    
    /// <summary>
    /// Sets the view to use.
    /// </summary>
    /// <param name="view">View to use.</param>
    private void SetView(string view)
    {
        // Update the visibility.
        foreach (var panel in this.panels)
        {
            panel.IsVisible = (panel.Name == view);
        }
    }
}