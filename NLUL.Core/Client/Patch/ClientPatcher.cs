/*
 * TheNexusAvenger
 *
 * Manages installing and uninstalling patches.
 */

using System.Collections.Generic;
using System.IO;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    /*
     * Enums for the patch names.
     */
    public enum ClientPatchName
    {
        ModLoader,
        TcpUdp,
        AutoTcpUdp,
    }
    
    /*
     * Class for the patcher.
     */
    public class ClientPatcher
    {
        private SystemInfo systemInfo;
        private GitHubManifest manifest;
        public readonly Dictionary<ClientPatchName,IPatch> patches;
        
        /*
         * Creates the client patcher.
         */
        public ClientPatcher(SystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
            this.manifest = new GitHubManifest(Path.Combine(systemInfo.ClientLocation,"GitHubPatches.json"));
            this.patches = new Dictionary<ClientPatchName,IPatch>()
            {
                {ClientPatchName.ModLoader,new ModLoader(systemInfo,this.manifest)},
                {ClientPatchName.TcpUdp,new TcpUdp(systemInfo,this.manifest)},
                {ClientPatchName.AutoTcpUdp,new AutoTcpUdp(systemInfo,this.manifest)},
            };
        }
        
        /*
         * Returns if an update is available for the patch.
         */
        public bool IsUpdateAvailable(ClientPatchName patchName)
        {
            return this.patches[patchName].IsUpdateAvailable();
        }
        
        /*
         * Returns if a patch is installed.
         */
        public bool IsInstalled(ClientPatchName patchName)
        {
            return this.patches[patchName].IsInstalled();
        }
        
        /*
         * Installs a patch.
         */
        public void Install(ClientPatchName patchName,bool force = false)
        {
            // Return if the patch is installed and up to date.
            var patch = this.patches[patchName];
            if (!force && patch.IsInstalled() && !patch.IsUpdateAvailable())
            {
                return;
            }
            
            // Install the patch.
            patch.Install();
        }
        
        /*
         * Uninstalls a patch.
         */
        public void Uninstall(ClientPatchName patchName)
        {
            // Return if the patch is not installed.
            var patch = this.patches[patchName];
            if (!patch.IsInstalled())
            {
                return;
            }
            
            // Uninstall the patch.
            patch.Uninstall();
        }
    }
}