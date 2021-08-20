using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Patches
{
    public class PatchesView : StackPanel
    {
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
            this.patchesList = this.Get<StackPanel>("PatchesList");
            
            // Add the patch frames.
            this.ReloadPatches();
            Client.ClientSourceChanged += this.ReloadPatches;
        }

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
                if (Client.ClientSource.Patches.FirstOrDefault(o => o.Name == patch.PatchEnum) == null) continue;
                var patchPanel = new PatchEntry();
                patchPanel.Patcher = patcher;
                patchPanel.PatchData = patch;
                this.patchesList.Children.Add(patchPanel);
                this.patchEntries.Add(patchPanel);
            }
        }
    }
}