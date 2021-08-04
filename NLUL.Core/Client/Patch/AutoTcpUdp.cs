using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using InfectedRose.Lvl;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    public class AutoTcpUdp : IPreLaunchPatch
    {
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
        /// Location of the mod folder.
        /// </summary>
        private readonly string modFolderLocation;
        
        /// <summary>
        /// Location of the disabled mod folder.
        /// </summary>
        private readonly string disableModFolderLocation;
        
        /// <summary>
        /// Location of the mod executable.
        /// </summary>
        private readonly string modLocation;
        
        /// <summary>
        /// Location of the disabled mod executable.
        /// </summary>
        private readonly string disabledModLocation;
        
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable => !this.repositoryEntry.IsTagUpToDate();

        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed => File.Exists(this.modLocation) || File.Exists(this.disabledModLocation);

        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        /// <param name="manifest">GitHub manifest of the client.</param>
        public AutoTcpUdp(SystemInfo systemInfo,GitHubManifest manifest)
        {
            this.systemInfo = systemInfo;
            this.manifest = manifest;
            this.repositoryEntry = manifest.GetEntry("lcdr/raknet_shim_dll", Path.Combine(systemInfo.SystemFileLocation, "autotcpudp"));
            this.modFolderLocation = Path.Combine(this.systemInfo.ClientLocation, "mods", "auto_raknet_replacer");
            this.disableModFolderLocation = Path.Combine(this.systemInfo.ClientLocation, "disabledmods", "auto_raknet_replacer");
            this.modLocation = Path.Combine(this.modFolderLocation, "mod.dll");
            this.disabledModLocation = Path.Combine(this.disableModFolderLocation, "mod.dll");
        }

        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install()
        {
            // Get the tag information.
            var tag = this.repositoryEntry.GetLatestTag();
            
            // Create the mod directory.
            if (!Directory.Exists(this.modFolderLocation))
            {
                Directory.CreateDirectory(this.modFolderLocation);
            }

            // Remove the existing mod.dll.
            if (File.Exists(this.modLocation))
            {
                File.Delete(this.modLocation);
            }
            
            // Download the mod.
            var client = new WebClient();
            client.DownloadFile("https://github.com/lcdr/raknet_shim_dll/releases/download/" + tag.name + "/mod.dll", modLocation);

            // Save the manifest.
            this.repositoryEntry.lastCommit = tag.commit;
            this.manifest.Save();
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall()
        {
            // Remove the mod directory.
            if (Directory.Exists(this.modFolderLocation))
            {
                Directory.Delete(this.modFolderLocation, true);
            }
            if (Directory.Exists(this.disableModFolderLocation))
            {
                Directory.Delete(this.disableModFolderLocation, true);
            }
        }
        
        /// <summary>
        /// Performs and operations between setting the
        /// boot.cfg and launching the client. This will
        /// yield launching the client.
        /// </summary>
        public void OnClientRequestLaunch()
        {
            // Determine the host to check.
            var bootConfig = LegoDataDictionary.FromString(File.ReadAllText(Path.Combine(this.systemInfo.ClientLocation, "boot.cfg")).Trim());
            var host = (string) bootConfig["AUTHSERVERIP"];
            Console.WriteLine("Check for TCP/UDP for: " + host);
            
            // Assume TCP/UDP if any port is specified.
            // Even if 1001, the stock client will not connect correctly.
            if (host.Contains(":"))
            {
                var portString = host.Remove(0, host.IndexOf(":", StringComparison.Ordinal) + 1).Trim();
                if (int.TryParse(portString, out var port))
                {
                    Console.WriteLine("Custom port " + port + " specified. Assuming TCP/UDP.");
                    this.Enable();
                    return;
                }
            }
            
            // Try to connect and disconnect from port 21836 (default TCP/UDP port).
            // Port 1001 is more likely to be used by other applications like games.
            try
            {
                // Enable TCP/UDP after a successful connect and close.
                var client = new TcpClient(host, 21836);
                client.Close();
                Console.WriteLine("Connection to default TCP/UDP port 21836 successful. Assuming TCP/UDP.");
                this.Enable();
            }
            catch (Exception)
            {
                // Disable TCP/UDP (assume RakNet).
                Console.WriteLine("Connection to default TCP/UDP port 21836 failed. Assuming not TCP/UDP.");
                this.Disable();
            }
        }
        
        /// <summary>
        /// Enables the TCP/UDP mod.
        /// </summary>
        private void Enable()
        {
            this.SwitchModDirectory(this.disableModFolderLocation, this.modFolderLocation);
        }
        
        /// <summary>
        /// Disables the TCP/UDP mod.
        /// </summary>
        private void Disable()
        {
            this.SwitchModDirectory(this.modFolderLocation, this.disableModFolderLocation);
        }
        
        /// <summary>
        /// Switches the directories of the mod.
        /// </summary>
        /// <param name="source">Source of the mod file.</param>
        /// <param name="target">New target of the mod file.</param>
        private void SwitchModDirectory(string source, string target)
        {
            var sourceMod = Path.Combine(source, "mod.dll");
            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }
            if (File.Exists(sourceMod))
            {
                File.Move(sourceMod, Path.Combine(target, "mod.dll"));
            }
            if (Directory.Exists(source))
            {
                Directory.Delete(source, true);
            }
        }
    }
}