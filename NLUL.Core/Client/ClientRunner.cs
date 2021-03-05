/*
 * TheNexusAvenger
 *
 * Downloads, patches, and launches the client.
 */

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
        private SystemInfo SystemInfo;
        private string DownloadLocation;
        private string ExtractLocation;
        private ClientPatcher clientPatcher;
        private ClientRuntime runtime;
        
        /*
         * Creates a Client instance.
         */
        public ClientRunner(SystemInfo systemInfo)
        {
            this.SystemInfo = systemInfo;
            this.DownloadLocation = Path.Combine(systemInfo.SystemFileLocation,"client.zip");
            this.ExtractLocation = Path.Combine(systemInfo.SystemFileLocation,"ClientExtract");
            this.clientPatcher = new ClientPatcher(systemInfo);
            this.runtime = new ClientRuntime(systemInfo);
        }
        
        /*
         * Returns the runtime for the client.
         */
        public IRuntime GetRuntime()
        {
            return this.runtime;
        }
        
        /*
         * Returns the client patcher.
         */
        public ClientPatcher GetPatcher()
        {
            return this.clientPatcher;
        }
        
        /*
         * Downloads the client.
         */
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
            Directory.CreateDirectory(Path.GetDirectoryName(this.SystemInfo.ClientLocation));
            var client = new WebClient();
            client.DownloadFile("https://s3.amazonaws.com/luclient/luclient.zip",this.DownloadLocation);
        }
        
        /*
         * Extracts the client files.
         */
        private void ExtractClient(bool force)
        {
            // Clean the client if forced.
            if (force && Directory.Exists(this.ExtractLocation))
            {
                Directory.Delete(this.ExtractLocation,true);
            }
            if (force && Directory.Exists(this.SystemInfo.ClientLocation))
            {
                Directory.Delete(this.SystemInfo.ClientLocation,true);
            }
            
            // Extract the files.
            Console.WriteLine("Extracting the client files.");
            if (!Directory.Exists(this.ExtractLocation))
            {
                ZipFile.ExtractToDirectory(this.DownloadLocation,this.ExtractLocation);
            }
            
            // Move the files.
            Directory.Move(Path.Combine(this.ExtractLocation,"LCDR Unpacked"),this.SystemInfo.ClientLocation);
            Directory.Delete(this.ExtractLocation);
        }
        
        /*
         * Tries to extract the client files. If it fails,
         * the client is re-downloaded.
         */
        public void TryExtractClient(bool force,Action<string> statusCallback = null)
        {
            // Return if the client was already extracted.
            if (!force && Directory.Exists(this.SystemInfo.ClientLocation))
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
        
        /*
         * Patches the client files with the default patches.
         */
        public void PatchClient()
        {
            this.clientPatcher.Install(ClientPatchName.ModLoader);
            this.clientPatcher.Install(ClientPatchName.AutoTcpUdp);
            this.clientPatcher.Install(ClientPatchName.RemoveDLUAd);
        }
        
        /*
         * Returns if the client extract can be verified.
         */
        public bool CanVerifyExtractedClient()
        {
            return File.Exists(this.DownloadLocation);
        }
        
        /*
         * Verifies if the extracting of the client
         * is valid. Throws an exception if the
         * verification failed.
         */
        public void VerifyExtractedClient()
        {
            // Return if the client can't be verified.
            if (!CanVerifyExtractedClient())
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
                    var filePath = Path.Combine(this.SystemInfo.ClientLocation, fileName);
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
        
        /*
         * Launches the client.
         */
        public void Launch(string host,bool waitForFinish = true)
        {
            // Set up the runtime if it isn't installed.
            if (!this.runtime.IsInstalled())
            {
                if (this.runtime.CanInstall())
                {
                    // Install the runtime.
                    this.runtime.Install();
                }
                else
                {
                    // Stop the launch if a valid runtime isn't set up.
                    Console.WriteLine("Failed to launch: " + this.runtime.GetManualRuntimeInstallMessage());
                    return;
                }
            }
            
            // Modify the boot file.
            Console.WriteLine("Setting to connect to \"" + host + "\"");
            var bootConfigLocation = Path.Combine(this.SystemInfo.ClientLocation,"boot.cfg");
            LegoDataDictionary bootConfig = null;
            try
            {
                bootConfig = LegoDataDictionary.FromString(File.ReadAllText(bootConfigLocation).Trim());
            }
            catch (FormatException)
            {
                bootConfig = LegoDataDictionary.FromString(File.ReadAllText(Path.Combine(this.SystemInfo.ClientLocation,"boot_backup.cfg")).Trim());
            }
            bootConfig["AUTHSERVERIP"] = host;
            File.WriteAllText(bootConfigLocation,bootConfig.ToString("\n"));
            
            // Apply any pre-launch patches.
            foreach (var patch in clientPatcher.patches)
            {
                if (patch.Value is IPreLaunchPatch preLaunchPatch)
                {
                    preLaunchPatch.OnClientRequestLaunch();
                }
            }
            
            // Launch the client.
            Console.WriteLine("Launching the client.");
            var clientProcess = this.runtime.RunApplication(Path.Combine(this.SystemInfo.ClientLocation,"legouniverse.exe"), this.SystemInfo.ClientLocation);
            clientProcess.Start();
            
            // Wait for the client to close.
            if (!waitForFinish) return;
            clientProcess.WaitForExit();
            Console.WriteLine("Client closed.");
        }
    }
}