using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nexus.LU.Launcher.Gui.Util;
using Nexus.LU.Launcher.State.Client;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.Gui.Component.Patches;

public class PatchesView : StackPanel
{
    /// <summary>
    /// Message for the client not being download.
    /// </summary>
    private readonly TextBlock notDownloadedMessage;
    
    /// <summary>
    /// Scroll view of the patches.
    /// </summary>
    private readonly ScrollViewer patchesScroll;
    
    /// <summary>
    /// Display list of the patches.
    /// </summary>
    private readonly StackPanel patchesList;

    /// <summary>
    /// Entry for adding archive patches.
    /// </summary>
    private readonly NewArchivePatchEntry newArchivePatchEntry;

    /// <summary>
    /// List of the patch entries.
    /// </summary>
    private readonly List<PatchEntry> patchEntries = new List<PatchEntry>();
    
    /// <summary>
    /// Creates a patches view.
    /// </summary>
    public PatchesView()
    {
        // Load the XAML.
        AvaloniaXamlLoader.Load(this);
        this.notDownloadedMessage = this.Get<TextBlock>("NotDownloadedMessage");
        this.patchesScroll = this.Get<ScrollViewer>("PatchesScroll");
        this.patchesList = this.Get<StackPanel>("PatchesList");
        
        // Add the patch frames.
        var clientState = ClientState.Get();
        foreach (var patch in clientState.Patches)
        {
            var patchPanel = new PatchEntry();
            patchPanel.PatchData = patch;
            this.patchEntries.Add(patchPanel);
            patch.StateChanged += (state) =>
            {
                this.RunMainThread(UpdateList);
            };
        }
        this.newArchivePatchEntry = new NewArchivePatchEntry();
        this.UpdateList();
        
        // Connect updating the patches visibility.
        clientState.LauncherStateChanged += (state) =>
        {
            this.RunMainThread(this.UpdateVisibility);
        };
        clientState.PatchAdded += (patch) =>
        {
            this.RunMainThread(() =>
            {
                var patchPanel = new PatchEntry();
                patchPanel.PatchData = patch;
                this.patchEntries.Add(patchPanel);
                patch.StateChanged += (state) =>
                {
                    this.RunMainThread(UpdateList);
                };
                this.UpdateList();
            });
        };
        this.UpdateVisibility();
    }

    /// <summary>
    /// Updates the visibility of the patches.
    /// </summary>
    private void UpdateVisibility()
    {
        var canPatch = File.Exists(Path.Combine(SystemInfo.GetDefault().ClientLocation, "legouniverse.exe"));
        this.notDownloadedMessage.IsVisible = !canPatch;
        this.patchesScroll.IsVisible = canPatch;
    }

    /// <summary>
    /// Updates the list of entries.
    /// </summary>
    private void UpdateList()
    {
        // Clear the patches.
        foreach (var entry in this.patchEntries)
        {
            if (!this.patchesList.Children.Contains(entry)) continue;
            this.patchesList.Children.Remove(entry);
        }
        this.patchesList.Children.Remove(this.newArchivePatchEntry);
            
        // Add the patches.
        foreach (var entry in this.patchEntries)
        {
            if (entry.PatchData.State == ExtendedPatchState.Incompatible) continue;
            this.patchesList.Children.Add(entry);
        }
        this.patchesList.Children.Add(this.newArchivePatchEntry);
    }
}