/*
 * TheNexusAvenger
 *
 * Patch for the mod loader.
 */

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    public class ModLoader : IPatch
    {
        private SystemInfo systemInfo;
        private GitHubManifest manifest;
        private GitHubManifestEntry repositoryEntry;
     
        /*
         * Creates the patch.
         */
        public ModLoader(SystemInfo systemInfo,GitHubManifest manifest)
        {
            this.systemInfo = systemInfo;
            this.manifest = manifest;
            this.repositoryEntry = manifest.GetEntry("lcdr/raknet_shim_dll",Path.Combine(systemInfo.SystemFileLocation,"raknet_modloader"));
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
            return File.Exists(Path.Join(this.systemInfo.ClientLocation,"dinput8.dll"));
        }
        
        /*
         * Installs the patch.
         */
        public void Install()
        {
            // Get the tag information.
            var tag = this.repositoryEntry.GetLatestTag();
            
            // Create the client directory if it doesn't exist.
            if (!Directory.Exists(this.systemInfo.ClientLocation))
            {
                Directory.CreateDirectory(this.systemInfo.ClientLocation);
            }
            
            // Download the mod loader ZIP.
            var client = new WebClient();
            var modDownloadDirectory = Path.Combine(this.systemInfo.SystemFileLocation, "modloader.zip"); 
            var modUncompressDirectory = Path.Combine(this.systemInfo.SystemFileLocation, "modloader");
            client.DownloadFile("https://github.com/lcdr/raknet_shim_dll/releases/download/" + tag.name + "/mod.zip",modDownloadDirectory);
            
            if (Directory.Exists(modUncompressDirectory))
            {
                Directory.Delete(modUncompressDirectory, true);
            }
            ZipFile.ExtractToDirectory(modDownloadDirectory,modUncompressDirectory);
            
            // Remove the existing dinput8.dll.
            var dinput8Location = Path.Join(this.systemInfo.ClientLocation,"dinput8.dll");
            if (File.Exists(dinput8Location))
            {
                File.Delete(dinput8Location);
            }

            // Replace the dinput8.dll file.
            var dinput8DownloadLocation = Directory.GetFiles(modUncompressDirectory, "dinput8.dll", SearchOption.AllDirectories)[0];
            File.Move(dinput8DownloadLocation, dinput8Location);

            // Create the mods directory if it doesn't exist.
            var modsDirectory = Path.Join(this.systemInfo.ClientLocation,"mods");
            if (!Directory.Exists(modsDirectory))
            {
                Directory.CreateDirectory(modsDirectory);
            }
            
            // Save the manifest.
            this.repositoryEntry.lastCommit = tag.commit;
            this.manifest.Save();
            
            // Clear the downloaded files.
            File.Delete(modDownloadDirectory);
            Directory.Delete(modUncompressDirectory,true);
        }
        
        /*
         * Uninstalls the patch.
         */
        public void Uninstall()
        {
            // Remove the mod loader DLL.
            File.Delete(Path.Combine(this.systemInfo.ClientLocation,"dinput8.dll"));
            
            // Remove the mods directory if it is empty.
            var modsDirectory = Path.Join(this.systemInfo.ClientLocation,"mods");
            if (Directory.GetDirectories(modsDirectory).Length == 0 && Directory.GetFiles(modsDirectory).Length == 0)
            {
                Directory.Delete(modsDirectory);
            }
        }
    }
}