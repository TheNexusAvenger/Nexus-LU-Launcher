/*
 * TheNexusAvenger
 *
 * Patch for RakNet.
 */

using System.IO;
using System.Net;
using NLUL.Core.Server.Util;

namespace NLUL.Core.Client.Patch
{
    public class RakNet : IPatch
    {
        private SystemInfo systemInfo;
        private GitHubManifest manifest;
        private GitHubManifestEntry repositoryEntry;
     
        /*
         * Creates the patch.
         */
        public RakNet(SystemInfo systemInfo,GitHubManifest manifest)
        {
            this.systemInfo = systemInfo;
            this.manifest = manifest;
            this.repositoryEntry = manifest.GetEntry("lcdr/raknet_shim_dll",Path.Combine(systemInfo.SystemFileLocation,"raknet"));
        }
        
        /*
         * Returns if an update is available.
         */
        public bool IsUpdateAvailable()
        {
            return !this.repositoryEntry.IsTagUpToDate();
        }
        
        /*
         * Returns if the patch is installed.
         */
        public bool IsInstalled()
        {
            return File.Exists(Path.Join(this.systemInfo.ClientLocation,"mods","raknet_replacer","mod.dll"));
        }
        
        /*
         * Installs the patch.
         */
        public void Install()
        {
            // Get the tag information.
            var tag = this.repositoryEntry.GetLatestTag();
            
            // Create the mod directory.
            var modDirectory = Path.Combine(this.systemInfo.ClientLocation,"mods","raknet_replacer");
            if (!Directory.Exists(modDirectory))
            {
                Directory.CreateDirectory(modDirectory);
            }
            
            // Remove the existing mod.dll.
            var modLocation = Path.Combine(modDirectory, "mod.dll");
            if (File.Exists(modLocation))
            {
                File.Delete(modLocation);
            }
            // Download the mod.
            var client = new WebClient();
            client.DownloadFile("https://github.com/lcdr/raknet_shim_dll/releases/download/" + tag.name + "/mod.dll",modLocation);

            // Save the manifest.
            this.repositoryEntry.lastCommit = tag.commit;
            this.manifest.Save();
        }
        
        /*
         * Uninstalls the patch.
         */
        public void Uninstall()
        {
            // Remove the mod directory.
            Directory.Delete(Path.Combine(this.systemInfo.ClientLocation,"mods","raknet_replacer"),true);
        }
    }
}