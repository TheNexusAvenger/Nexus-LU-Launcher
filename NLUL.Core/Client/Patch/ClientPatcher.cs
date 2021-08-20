using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public readonly List<IPatch> Patches;

        /// <summary>
        /// Manifest of the client patcher.
        /// </summary>
        public readonly GitHubManifest Manifest;
        
        /// <summary>
        /// Creates the client patcher.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        public ClientPatcher(SystemInfo systemInfo)
        {
            this.Manifest = new GitHubManifest(Path.Combine(systemInfo.ClientLocation, "GitHubPatches.json"));
            this.Patches = new List<IPatch>()
            {
                new ModLoader(systemInfo, this.Manifest),
                new TcpUdp(systemInfo, this.Manifest),
                new AutoTcpUdp(systemInfo, this.Manifest),
                new FixAvantGardensSurvivalCrash(systemInfo),
                new FixAssemblyVendorHologram(systemInfo),
                new RemoveDLUAd(systemInfo),
            };
        }

        /// <summary>
        /// Returns the patch for the given name.
        /// </summary>
        /// <param name="patchName">Patch name to search for.</param>
        private IPatch GetPatch(ClientPatchName patchName)
        {
            return this.Patches.First(patch => patch.PatchEnum == patchName);
        }
        
        /// <summary>
        /// Returns if an update is available for the patch.
        /// </summary>
        /// <param name="patchName">Patch to check for.</param>
        /// <returns>Whether an update is available for the patch.</returns>
        public bool IsUpdateAvailable(ClientPatchName patchName)
        {
            return this.GetPatch(patchName).UpdateAvailable;
        }
        
        /// <summary>
        /// Returns if a patch is installed
        /// </summary>
        /// <param name="patchName"></param>
        /// <returns>Whether the patch is installed.</returns>
        public bool IsInstalled(ClientPatchName patchName)
        {
            return this.GetPatch(patchName).Installed;
        }
        
        /// <summary>
        /// Installs a patch.
        /// </summary>
        /// <param name="patchName">Name of the patch.</param>
        public void Install(ClientPatchName patchName)
        {
            // Return if the patch is installed and up to date.
            var patch = this.GetPatch(patchName);
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
            var patch = this.GetPatch(patchName);
            if (!patch.Installed)
            {
                return;
            }
            
            // Uninstall the patch.
            patch.Uninstall();
        }
    }
}