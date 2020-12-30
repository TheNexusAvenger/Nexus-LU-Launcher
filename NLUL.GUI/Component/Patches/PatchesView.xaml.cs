/*
 * TheNexusAvenger
 *
 * View for the patches screen.
 */

using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NLUL.Core.Client.Patch;
using NLUL.GUI.State;

namespace NLUL.GUI.Component.Patches
{
    public class PatchData
    {
        public string PatchName;
        public string PatchDescription;
        public ClientPatchName PatchEnum;
        
        public static readonly List<PatchData> Patches = new List<PatchData>()
        {
            new PatchData() {
                PatchName = "Mod Loader",
                PatchDescription = "Allows the installation of client mods.",
                PatchEnum = ClientPatchName.ModLoader,
            },
            new PatchData() {
                PatchName = "TCP/UDP Shim",
                PatchDescription = "Enables connecting to community-run Lego Universe servers. Requires the Mod Loader to be installed.",
                PatchEnum = ClientPatchName.TcpUdp,
            },
        };
    }
    
    public class PatchesView : StackPanel
    {
        /*
         * Creates a patches view.
         */
        public PatchesView()
        {
            // Load the XAML.
            AvaloniaXamlLoader.Load(this);
            
            // Add the patch frames.
            var patcher = new ClientPatcher(ProgramSystemInfo.SystemInfo);
            var patchesList = this.Get<StackPanel>("PatchesList");
            foreach (var patch in PatchData.Patches)
            {
                var patchPanel = new PatchEntry();
                patchPanel.Patcher = patcher;
                patchPanel.PatchData = patch;
                patchesList.Children.Add(patchPanel);
            }
        }
    }
}