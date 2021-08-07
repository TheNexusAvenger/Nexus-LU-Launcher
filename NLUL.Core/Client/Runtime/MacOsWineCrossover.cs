using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace NLUL.Core.Client.Runtime
{
    public class MacOsWineCrossover : IRuntime
    {
        /// <summary>
        /// Info of the system.
        /// </summary>
        private readonly SystemInfo systemInfo;
        
        /// <summary>
        /// Name of the runtime.
        /// </summary>
        public string Name => "WINE Crossover";

        /// <summary>
        /// Whether the emulator is supported on the current platform.
        /// </summary>
        public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        /// <summary>
        /// Whether the emulator can be automatically installed.
        /// </summary>
        public bool CanInstall => true;
        
        /// <summary>
        /// Whether the emulator is installed.
        /// </summary>
        /// <returns></returns>
        public bool IsInstalled => File.Exists(Path.Combine(this.systemInfo.SystemFileLocation,"Wine","bin","wine32on64"));

        /// <summary>
        /// The message to display to the user if the runtime
        /// isn't installed and can't be automatically installed.
        /// </summary>
        public string ManualRuntimeInstallMessage => null;
        
        /// <summary>
        /// Creates the runtime.
        /// </summary>
        /// <param name="systemInfo">Information of the system.</param>
        public MacOsWineCrossover(SystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
        }
        
        /// <summary>
        /// Attempts to install the emulator.
        /// </summary>
        public void Install()
        {
            // Download the WINE Crossover app.
            var wineDownloadLocation = Path.Combine(this.systemInfo.SystemFileLocation,"wine-crossover.tar.7z");
            if (!File.Exists(wineDownloadLocation))
            {
                var client = new WebClient();
                client.DownloadFile("https://github.com/Gcenx/homebrew-wine/releases/download/20.0.2/wine-crossover-20.0.2-osx64.tar.7z",wineDownloadLocation);
            }
            
            var wineDirectoryTarParentLocation = Path.Combine(this.systemInfo.SystemFileLocation,"wine-crossover-tar");
            var wineDirectoryTarLocation = Path.Combine(this.systemInfo.SystemFileLocation,"wine-crossover-tar","wine-crossover-20.0.2-osx64.tar");
            var wineDirectoryExtractedLocation = Path.Combine(this.systemInfo.SystemFileLocation,"wine-crossover-extracted");
            var wineInitialDirectoryLocation = Path.Combine(wineDirectoryExtractedLocation,"Wine Crossover.app","Contents","Resources","wine");
            var wineTargetDirectoryLocation = Path.Combine(this.systemInfo.SystemFileLocation,"Wine");
            
            // Extract the WINE .tar.7z to a .tar.
            try
            {
                ExtractArchive(wineDownloadLocation, wineDirectoryTarParentLocation);
            }
            catch (Exception exception)
            {
                // Delete the download and restart.
                if (!(exception is NotImplementedException) && !(exception is InvalidOperationException)) throw;
                File.Delete(wineDownloadLocation);
                Install();
                return;
            }
            
            // Extract the WINE .tar using the tar command.
            // This is done with the system tar command to preserve symbolic links.
            if (Directory.Exists(wineDirectoryExtractedLocation))
            {
                Directory.Delete(wineDirectoryExtractedLocation, true);
            }
            Directory.CreateDirectory(wineDirectoryExtractedLocation);
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "tar",
                    Arguments = "-xf \"" + wineDirectoryTarLocation.Replace("\"","\\\"") + "\" -C \"" + wineDirectoryExtractedLocation.Replace("\"","\\\"") + "\"",
                }
            };
            process.Start();
            process.WaitForExit();
            
            // Move the WINE directory.
            if (Directory.Exists(wineTargetDirectoryLocation))
            {
                Directory.Delete(wineTargetDirectoryLocation, true);
            }
            Directory.Move(wineInitialDirectoryLocation, wineTargetDirectoryLocation);
            
            // Clear the files.
            File.Delete(wineDownloadLocation);
            Directory.Delete(wineDirectoryTarParentLocation, true);
            Directory.Delete(wineDirectoryExtractedLocation, true);
        }
        
        /// <summary>
        /// Runs an application in the emulator.
        /// </summary>
        /// <param name="executablePath">Path of the executable to run.</param>
        /// <param name="workingDirectory">Working directory to run the executable in.</param>
        /// <returns>The process of the runtime.</returns>
        public Process RunApplication(string executablePath, string workingDirectory)
        {
            var clientProcess = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(this.systemInfo.SystemFileLocation,"Wine","bin","wine32on64"),
                    Arguments = executablePath,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                }
            };
            clientProcess.StartInfo.EnvironmentVariables.Add("WINEDLLOVERRIDES","dinput8.dll=n,b");
            return clientProcess;
        }
        
        /*
         * Extracts from 1 directory to another.
         */
        private void ExtractArchive(string source, string target)
        {
            using var archive = ArchiveFactory.Open(source);
            foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
            {
                entry.WriteToDirectory(target,
                    new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                    });
            }
        }
    }
}