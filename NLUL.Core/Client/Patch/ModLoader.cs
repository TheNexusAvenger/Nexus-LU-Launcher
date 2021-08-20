using System.IO;
using System.IO.Compression;
using System.Net;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    public class ModLoader : IPatch
    {
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name => "Mod Loader";
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        public string Description => "Allows the installation of client mods.";

        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum => ClientPatchName.ModLoader;
        
        /// <summary>
        /// System info of the client.
        /// </summary>
        private readonly SystemInfo systemInfo;
        
        /// <summary>
        /// GitHub manifest of the client.
        /// </summary>
        private readonly GitHubManifest manifest;
        
        /// <summary>
        /// GitHub manifest entry for the patch.
        /// </summary>
        private readonly GitHubManifestEntry repositoryEntry;
     
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable => !this.repositoryEntry.IsTagUpToDate();

        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed => File.Exists(Path.Join(this.systemInfo.ClientLocation, "dinput8.dll"));
        
        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        /// <param name="manifest">GitHub manifest of the client.</param>
        public ModLoader(SystemInfo systemInfo, GitHubManifest manifest)
        {
            this.systemInfo = systemInfo;
            this.manifest = manifest;
            this.repositoryEntry = manifest.GetEntry("lcdr/raknet_shim_dll", Path.Combine(systemInfo.SystemFileLocation, "raknet_modloader"));
        }

        /// <summary>
        /// Installs the patch.
        /// </summary>
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
            client.DownloadFile("https://github.com/lcdr/raknet_shim_dll/releases/download/" + tag.name + "/mod.zip", modDownloadDirectory);
            
            if (Directory.Exists(modUncompressDirectory))
            {
                Directory.Delete(modUncompressDirectory, true);
            }
            ZipFile.ExtractToDirectory(modDownloadDirectory,modUncompressDirectory);
            
            // Remove the existing dinput8.dll.
            var dinput8Location = Path.Join(this.systemInfo.ClientLocation, "dinput8.dll");
            if (File.Exists(dinput8Location))
            {
                File.Delete(dinput8Location);
            }

            // Replace the dinput8.dll file.
            var dinput8DownloadLocation = Directory.GetFiles(modUncompressDirectory, "dinput8.dll", SearchOption.AllDirectories)[0];
            File.Move(dinput8DownloadLocation, dinput8Location);

            // Create the mods directory if it doesn't exist.
            var modsDirectory = Path.Join(this.systemInfo.ClientLocation, "mods");
            if (!Directory.Exists(modsDirectory))
            {
                Directory.CreateDirectory(modsDirectory);
            }
            
            // Save the manifest.
            this.repositoryEntry.LastCommit = tag.commit;
            this.manifest.Save();
            
            // Clear the downloaded files.
            File.Delete(modDownloadDirectory);
            Directory.Delete(modUncompressDirectory,true);
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall()
        {
            // Remove the mod loader DLL.
            File.Delete(Path.Combine(this.systemInfo.ClientLocation, "dinput8.dll"));
            
            // Remove the mods directory if it is empty.
            var modsDirectory = Path.Join(this.systemInfo.ClientLocation, "mods");
            if (Directory.GetDirectories(modsDirectory).Length == 0 && Directory.GetFiles(modsDirectory).Length == 0)
            {
                Directory.Delete(modsDirectory);
            }
        }
    }
}