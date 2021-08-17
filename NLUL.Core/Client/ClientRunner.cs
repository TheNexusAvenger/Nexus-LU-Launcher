using System;
using System.Collections.Generic;
using System.IO;
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
        /// Download method for the client.
        /// </summary>
        private DownloadMethod downloadMethod;
        
        /// <summary>
        /// Whether the client extract can be verified.
        /// </summary>
        public bool CanVerifyExtractedClient => this.downloadMethod != null && this.downloadMethod.CanVerifyExtractedClient;
        
        /// <summary>
        /// Patcher for the client runner.
        /// </summary>
        public ClientRuntime Runtime { get; }
        
        /// <summary>
        /// Patcher for the client runner.
        /// </summary>
        public ClientPatcher Patcher { get; }

        /// <summary>
        /// Size of the client to download. Initially set to the rough size of the
        /// unpacked client so that it has a non-zero fallback. It is set when a
        /// download starts.
        /// </summary>
        public long ClientDownloadSize => this.downloadMethod != null && this.downloadMethod.ClientDownloadSize != default ? this.downloadMethod.ClientDownloadSize : 4513866950;
        
        /// <summary>
        /// Size of the client that has been downloaded.
        /// </summary>
        public long DownloadedClientSize => this.downloadMethod?.DownloadedClientSize ?? 0;

        /// <summary>
        /// Source of the client to download.
        /// </summary>
        public ClientSourceEntry ClientSource { get; private set; }

        /// <summary>
        /// Event for the state changing.
        /// </summary>
        public event EventHandler<string> DownloadStateChanged;

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
                    Name = ClientPatchName.ModLoader,
                    Default = true,
                },
                new ClientPatchEntry()
                {
                    Name = ClientPatchName.AutoTcpUdp,
                    Default = true,
                },
                new ClientPatchEntry()
                {
                    Name = ClientPatchName.TcpUdp,
                    Default = false,
                },
                new ClientPatchEntry()
                {
                    Name = ClientPatchName.FixAssemblyVendorHologram,
                    Default = true,
                },
                new ClientPatchEntry()
                {
                    Name = ClientPatchName.RemoveDLUAd,
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
            this.SetSource(this.testSourceEntry);
        }

        /// <summary>
        /// Sets the download source.
        /// </summary>
        /// <param name="source">Source to use.</param>
        public void SetSource(ClientSourceEntry source)
        {
            // Set the download source.
            if (source.Method == "zip")
            {
                this.downloadMethod = new ZipDownloadMethod(this.systemInfo, source);
            }
            else
            {
                throw new InvalidOperationException("Unsupported method: " + source.Method);
            }
            
            // Set up the source and events.
            this.ClientSource = source;
            this.downloadMethod.DownloadStateChanged += (_, state) =>
            {
                this.DownloadStateChanged?.Invoke(null, state);
            };
        }
        
        /// <summary>
        /// Tries to download and extract the client files. If it fails,
        /// the client is re-downloaded.
        /// </summary>
        public void Download(Action<string> statusCallback = null)
        {
            this.downloadMethod.Download();
        }
        
        /// <summary>
        /// Patches the client files with the default patches.
        /// </summary>
        public void PatchClient()
        {
            foreach (var patchEntry in this.ClientSource.Patches)
            {
                if (!patchEntry.Default) continue;
                this.Patcher.Install(patchEntry.Name);
            }
        }
        
        /// <summary>
        /// Verifies the extracted client.
        /// </summary>
        /// <returns>Whether the client was verified.</returns>
        public bool VerifyExtractedClient()
        {
            return this.downloadMethod.Verify();
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
                    return;
                }
            }
            
            // Modify the boot file.
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
            var clientProcess = this.Runtime.RunApplication(Path.Combine(this.systemInfo.ClientLocation, "legouniverse.exe"), this.systemInfo.ClientLocation);
            clientProcess.Start();
            
            // Wait for the client to close.
            if (!waitForFinish) return;
            clientProcess.WaitForExit();
        }
    }
}