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
                PatchDescription = "Enables connecting to community-run LEGO Universe servers that use TCP/UDP. Requires the Mod Loader to be installed.",
                PatchEnum = ClientPatchName.TcpUdp,
            },
            new PatchData() {
                PatchName = "Auto TCP/UDP Shim",
                PatchDescription = "Enables connecting to community-run LEGO Universe servers that may or may not use TCP/UDP. This is automatically managed for the requested server. Requires the Mod Loader to be installed. Do not install with TCP/UDP Shim.",
                PatchEnum = ClientPatchName.AutoTcpUdp,
            },
            new PatchData()
            {
                PatchName = "Fix Assembly Vendor Hologram",
                PatchDescription = "Fixes the Assembly vendor at Nimbus Station showing a Missing NIF error.",
                PatchEnum = ClientPatchName.FixAssemblyVendorHologram,
            },
            new PatchData() {
                PatchName = "Remove DLU Ad",
                PatchDescription = "Removes the advertisement for DLU from the zone loading screen.",
                PatchEnum = ClientPatchName.RemoveDLUAd,
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
            var patcher = Client.GetPatcher();
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