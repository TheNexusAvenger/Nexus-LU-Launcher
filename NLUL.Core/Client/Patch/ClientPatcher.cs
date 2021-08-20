using System.Collections.Generic;
using System.IO;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    /// <summary>
    /// Enums for the patch names.
    /// </summary>
    public enum ClientPatchName
    {
        ModLoader,
        TcpUdp,
        AutoTcpUdp,
        FixAssemblyVendorHologram,
        RemoveDLUAd,
        FixAvantGardensSurvivalCrash,
    }
    
    /// <summary>
    /// Class for the patcher.
    /// </summary>
    public class ClientPatcher
    {
        /// <summary>
        /// Patches that can be applied to the client.
        /// </summary>
        public readonly Dictionary<ClientPatchName,IPatch> patches;
        
        /// <summary>
        /// Creates the client patcher.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        public ClientPatcher(SystemInfo systemInfo)
        {
            var manifest = new GitHubManifest(Path.Combine(systemInfo.ClientLocation, "GitHubPatches.json"));
            this.patches = new Dictionary<ClientPatchName,IPatch>()
            {
                {ClientPatchName.ModLoader, new ModLoader(systemInfo, manifest)},
                {ClientPatchName.TcpUdp, new TcpUdp(systemInfo, manifest)},
                {ClientPatchName.AutoTcpUdp, new AutoTcpUdp(systemInfo, manifest)},
                {ClientPatchName.FixAssemblyVendorHologram, new FixAssemblyVendorHologram(systemInfo)},
                {ClientPatchName.RemoveDLUAd, new RemoveDLUAd(systemInfo)},
                {ClientPatchName.FixAvantGardensSurvivalCrash, new FixAvantGardensSurvivalCrash(systemInfo)},
            };
        }
        
        /// <summary>
        /// Returns if an update is available for the patch.
        /// </summary>
        /// <param name="patchName">Patch to check for.</param>
        /// <returns>Whether an update is available for the patch.</returns>
        public bool IsUpdateAvailable(ClientPatchName patchName)
        {
            return this.patches[patchName].UpdateAvailable;
        }
        
        /// <summary>
        /// Returns if a patch is installed
        /// </summary>
        /// <param name="patchName"></param>
        /// <returns>Whether the patch is installed.</returns>
        public bool IsInstalled(ClientPatchName patchName)
        {
            return this.patches[patchName].Installed;
        }
        
        /// <summary>
        /// Installs a patch.
        /// </summary>
        /// <param name="patchName">Name of the patch.</param>
        public void Install(ClientPatchName patchName)
        {
            // Return if the patch is installed and up to date.
            var patch = this.patches[patchName];
            if (patch.Installed && !patch.UpdateAvailable)
            {
                return;
            }
            
            // Install the patch.
            patch.Install();
        }
        
        /// <summary>
        /// Uninstalls a patch.
        /// </summary>
        /// <param name="patchName">Name of the patch.</param>
        public void Uninstall(ClientPatchName patchName)
        {
            // Return if the patch is not installed.
            var patch = this.patches[patchName];
            if (!patch.Installed)
            {
                return;
            }
            
            // Uninstall the patch.
            patch.Uninstall();
        }
    }
}