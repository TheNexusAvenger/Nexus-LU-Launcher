using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using InfectedRose.Lvl;
using NLUL.Core.Client.Download;
using NLUL.Core.Client.Patch;
using NLUL.Core.Client.Runtime;
using NLUL.Core.Client.Source;

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
        /// Test source entry.
        /// TODO: Remove
        /// </summary>
        private ClientSourceEntry testSourceEntry { get; } = new ClientSourceEntry()
        {
            Name = "LCDR Unpacked",
            Type = "Unpacked",
            Url = "http://localhost:8000/luclient.zip",
            Method = "zip",
            Patches = new List<ClientPatchEntry>()
            {
                new ClientPatchEntry()
                {
                    Name = "ModLoader",
                    Default = true,
                },
                new ClientPatchEntry()
                {
                    Name = "AutoTcpUdp",
                    Default = true,
                },
                new ClientPatchEntry()
                {
                    Name = "TcpUdp",
                    Default = false,
                },
                new ClientPatchEntry()
                {
                    Name = "FixAssemblyVendorHologram",
                    Default = true,
                },
                new ClientPatchEntry()
                {
                    Name = "RemoveDLUAd",
                    Default = true,
                },
            }
        };
        
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
        /// Tries to extract the client files. If it fails,
        /// the client is re-downloaded.
        /// </summary>
        /// <param name="statusCallback">Callback for the status of extracting.</param>
        public void TryExtractClient(Action<string> statusCallback = null)
        {
            var downloadMethod = new ZipDownloadMethod(this.systemInfo);
            downloadMethod.DownloadStateChanged += (sender, s) =>
            {
                statusCallback?.Invoke(s);
            };
            downloadMethod.Download(testSourceEntry);
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
            var downloadMethod = new ZipDownloadMethod(this.systemInfo);
            var errorFound = false;
            downloadMethod.DownloadStateChanged += (sender, s) =>
            {
                if (s != "VerifyFailed") return;
                errorFound = true;
            };
            downloadMethod.Download(testSourceEntry);

            if (!errorFound) return; 
            throw new FileNotFoundException("File not found in extracted client");
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