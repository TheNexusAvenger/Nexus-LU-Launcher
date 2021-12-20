using System.IO;
using System.Net;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    public class TcpUdp : IPatch
    {
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name => "TCP/UDP Shim";
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        public string Description => "Enables connecting to community-run LEGO Universe servers that use TCP/UDP. Requires the Mod Loader to be installed.";
        
        /// <summary>
        /// Whether the patch is hidden in the list of patches.
        /// </summary>
        public bool Hidden => false;
        
        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum => ClientPatchName.TcpUdp;
        
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
        public bool Installed => File.Exists(Path.Join(this.systemInfo.ClientLocation,"mods", "raknet_replacer", "mod.dll"));
        
        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        /// <param name="manifest">GitHub manifest of the client.</param>
        public TcpUdp(SystemInfo systemInfo, GitHubManifest manifest)
        {
            this.systemInfo = systemInfo;
            this.manifest = manifest;
            this.repositoryEntry = manifest.GetEntry("lcdr/raknet_shim_dll", Path.Combine(systemInfo.SystemFileLocation, "tcpudp"));
        }
        
        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install()
        {
            // Get the tag information.
            var tag = this.repositoryEntry.GetLatestTag();
            
            // Create the mod directory.
            var modDirectory = Path.Combine(this.systemInfo.ClientLocation, "mods", "raknet_replacer");
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
            this.repositoryEntry.LastCommit = tag.commit;
            this.manifest.Save();
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall()
        {
            // Remove the mod directory.
            Directory.Delete(Path.Combine(this.systemInfo.ClientLocation,"mods","raknet_replacer"),true);
        }
    }
}