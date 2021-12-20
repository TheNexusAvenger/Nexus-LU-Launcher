using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.Core;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Patches
{
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
            this.ReloadPatches();
            Client.StateChanged += () =>
            {
                if (Client.State != PlayState.Ready) return;
                this.ReloadPatches();
            };
            
            // Connect updating the patches visibility.
            Client.StateChanged += this.UpdateVisibility;
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
        /// Reloads the patches.
        /// </summary>
        private void ReloadPatches()
        {
            // Clear the patches.
            foreach (var entry in this.patchEntries)
            {
                this.patchesList.Children.Remove(entry);
            }
            this.patchEntries.Clear();
            
            // Add the patches.
            var patcher = Client.Patcher;
            foreach (var patch in Client.Patcher.Patches)
            {
                if (patch.Hidden) continue;
                var patchPanel = new PatchEntry();
                patchPanel.Patcher = patcher;
                patchPanel.PatchData = patch;
                this.patchesList.Children.Add(patchPanel);
                this.patchEntries.Add(patchPanel);
            }
        }
    }
}