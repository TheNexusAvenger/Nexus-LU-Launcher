using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using InfectedRose.Lvl;
using NLUL.Core.Client.Patch;
using NLUL.Core.Client.Runtime;

namespace NLUL.Core.Client
{
    public class ClientRunner
    {
        /// <summary>
        /// Information of the system.
        /// </summary>
        private readonly SystemInfo systemInfo;
        
        /// <summary>
        /// Download location of the client.
        /// </summary>
        private string DownloadLocation => Path.Combine(systemInfo.SystemFileLocation, "client.zip");
        
        /// <summary>
        /// Extract location of the client.
        /// </summary>
        private string ExtractLocation => Path.Combine(systemInfo.SystemFileLocation, "ClientExtract");
        
        /// <summary>
        /// Whether the client extract can be verified.
        /// </summary>
        public bool CanVerifyExtractedClient => File.Exists(this.DownloadLocation);
        
        /// <summary>
        /// Patcher for the client runner.
        /// </summary>
        public ClientRuntime Runtime { get; }
        
        /// <summary>
        /// Patcher for the client runner.
        /// </summary>
        public ClientPatcher Patcher { get; }
        
        /// <summary>
        /// Creates a Client instance.
        /// </summary>
        /// <param name="systemInfo">Information of the system.</param>
        public ClientRunner(SystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
            this.Patcher = new ClientPatcher(systemInfo);
            this.Runtime = new ClientRuntime(systemInfo);
        }
        
        /// <summary>
        /// Downloads the client.
        /// </summary>
        /// <param name="force">Whether to force download.</param>
        public void DownloadClient(bool force)
        {
            // Return if the client exists and isn't forced.
            if (!force && File.Exists(this.DownloadLocation))
            {
                Console.WriteLine("Client was already downloaded.");
                return;
            }
            
            // Delete the existing download if it exists.
            if (File.Exists(this.DownloadLocation))
            {
                File.Delete(this.DownloadLocation);
            }
            
            // Download the client.
            Console.WriteLine("Downloading the Lego Universe client.");
            Directory.CreateDirectory(Path.GetDirectoryName(this.systemInfo.ClientLocation));
            var client = new WebClient();
            client.DownloadFile("https://s3.amazonaws.com/luclient/luclient.zip",this.DownloadLocation);
        }
        
        /// <summary>
        /// Extracts the client files.
        /// </summary>
        /// <param name="force">Whether to force extract.</param>
        private void ExtractClient(bool force)
        {
            // Clean the client if forced.
            if (force && Directory.Exists(this.ExtractLocation))
            {
                Directory.Delete(this.ExtractLocation, true);
            }
            if (force && Directory.Exists(this.systemInfo.ClientLocation))
            {
                Directory.Delete(this.systemInfo.ClientLocation, true);
            }
            
            // Extract the files.
            Console.WriteLine("Extracting the client files.");
            if (!Directory.Exists(this.ExtractLocation))
            {
                ZipFile.ExtractToDirectory(this.DownloadLocation, this.ExtractLocation);
            }
            
            // Move the files.
            Directory.Move(Path.Combine(this.ExtractLocation,"LCDR Unpacked"), this.systemInfo.ClientLocation);
            Directory.Delete(this.ExtractLocation);
        }
        
        /// <summary>
        /// Tries to extract the client files. If it fails,
        /// the client is re-downloaded.
        /// </summary>
        /// <param name="force">Whether to force extract.</param>
        /// <param name="statusCallback">Callback for the status of extracting.</param>
        public void TryExtractClient(bool force, Action<string> statusCallback = null)
        {
            // Return if the client was already extracted.
            if (!force && Directory.Exists(this.systemInfo.ClientLocation))
            {
                Console.WriteLine("Client was already extracted.");
                return;
            }
            
            // Download the client if not done already.
            statusCallback?.Invoke("Download");
            this.DownloadClient(false);
            
            // Extract the files.
            try
            {
                Console.WriteLine("Trying to extract client files.");
                statusCallback?.Invoke("Extract");
                this.ExtractClient(force);
            }
            catch (InvalidDataException)
            {
                Console.WriteLine("Failed to extract the files (download corrupted); retrying download.");
                statusCallback?.Invoke("Download");
                this.DownloadClient(true);
                statusCallback?.Invoke("Extract");
                this.ExtractClient(force);
            }
            
            // Verify the client was extracted.
            statusCallback?.Invoke("Verify");
            this.VerifyExtractedClient();
        }
        
        /// <summary>
        /// Patches the client files with the default patches.
        /// </summary>
        public void PatchClient()
        {
            this.Patcher.Install(ClientPatchName.ModLoader);
            this.Patcher.Install(ClientPatchName.AutoTcpUdp);
            this.Patcher.Install(ClientPatchName.FixAssemblyVendorHologram);
            this.Patcher.Install(ClientPatchName.RemoveDLUAd);
        }
        
        /// <summary>
        /// Verifies if the extracting of the client
        /// is valid. Throws an exception if the
        /// verification failed.
        /// </summary>
        public void VerifyExtractedClient()
        {
            // Return if the client can't be verified.
            if (!this.CanVerifyExtractedClient)
            {
                return;
            }
            
            // Verify the files exist and throw an exception if a file is missing.
            using (var zipFile = ZipFile.OpenRead(this.DownloadLocation))
            {
                var entries = zipFile.Entries;
                foreach (var entry in entries)
                {
                    // Remove "LCDR Unpacked" from the file name.
                    var fileName = entry.FullName;
                    if (fileName.ToLower().StartsWith("lcdr unpacked"))
                    {
                        fileName = fileName.Substring(fileName.IndexOf("/", StringComparison.Ordinal) + 1);
                    }
                    
                    // Throw an exception if the file is missing.
                    var filePath = Path.Combine(this.systemInfo.ClientLocation, fileName);
                    if (entry.Length != 0 &&  !File.Exists(filePath))
                    {
                        throw new FileNotFoundException("File not found in extracted client: " + filePath);
                    }
                }
            }
            
            // Delete the client archive.
            if (File.Exists(this.DownloadLocation))
            {
                File.Delete(this.DownloadLocation);
            }
        }
        
        /// <summary>
        /// Launches the client.
        /// </summary>
        /// <param name="host">Host to launch.</param>
        /// <param name="waitForFinish">Whether to wait for the client to close.</param>
        public void Launch(string host, bool waitForFinish = true)
        {
            // Set up the runtime if it isn't installed.
            if (!this.Runtime.IsInstalled)
            {
                if (this.Runtime.CanInstall)
                {
                    // Install the runtime.
                    this.Runtime.Install();
                }
                else
                {
                    // Stop the launch if a valid runtime isn't set up.
                    Console.WriteLine("Failed to launch: " + this.Runtime.ManualRuntimeInstallMessage);
                    return;
                }
            }
            
            // Modify the boot file.
            Console.WriteLine("Setting to connect to \"" + host + "\"");
            var bootConfigLocation = Path.Combine(this.systemInfo.ClientLocation, "boot.cfg");
            LegoDataDictionary bootConfig = null;
            try
            {
                bootConfig = LegoDataDictionary.FromString(File.ReadAllText(bootConfigLocation).Trim());
            }
            catch (FormatException)
            {
                bootConfig = LegoDataDictionary.FromString(File.ReadAllText(Path.Combine(this.systemInfo.ClientLocation, "boot_backup.cfg")).Trim());
            }
            bootConfig["AUTHSERVERIP"] = host;
            File.WriteAllText(bootConfigLocation,bootConfig.ToString("\n"));
            
            // Apply any pre-launch patches.
            foreach (var patch in Patcher.patches)
            {
                if (patch.Value is IPreLaunchPatch preLaunchPatch)
                {
                    preLaunchPatch.OnClientRequestLaunch();
                }
            }
            
            // Launch the client.
            Console.WriteLine("Launching the client.");
            var clientProcess = this.Runtime.RunApplication(Path.Combine(this.systemInfo.ClientLocation, "legouniverse.exe"), this.systemInfo.ClientLocation);
            clientProcess.Start();
            
            // Wait for the client to close.
            if (!waitForFinish) return;
            clientProcess.WaitForExit();
            Console.WriteLine("Client closed.");
        }
    }
}