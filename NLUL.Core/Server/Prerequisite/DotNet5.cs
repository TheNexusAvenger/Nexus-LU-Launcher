/*
 * TheNexusAvenger
 *
 * Prerequisite that requires .NET 5.
 * Downloads the binaries to a known location.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace NLUL.Core.Server.Prerequisite
{
    public class DotNet5 : IPrerequisite
    {
        public static readonly Dictionary<OSPlatform,Dictionary<Architecture,string>> DOTNET_5_DOWNLOADS =
            new Dictionary<OSPlatform,Dictionary<Architecture,string>>()
            {
                {
                    OSPlatform.Windows,
                    new Dictionary<Architecture,string>()
                    {
                        {Architecture.X64,"https://download.visualstudio.microsoft.com/download/pr/178989cb-2bd9-4da8-881f-1acde0d4386c/5cdcc54c9d8f004ab748397a685d5d1b/dotnet-sdk-5.0.103-win-x64.zip"},
                        {Architecture.X86,"https://download.visualstudio.microsoft.com/download/pr/5696bb86-54a9-4d91-b34d-5ff4cf2daac4/c868e23c87303018994c934b2758ab06/dotnet-sdk-5.0.103-win-x86.zip"},
                        {Architecture.Arm64,"https://download.visualstudio.microsoft.com/download/pr/3fd92d44-eace-490d-aa9d-f7aef699162e/501e8fdd1438b3795afc55ab72397143/dotnet-sdk-5.0.103-win-arm64.zip"},
                    }
                },
                {
                    OSPlatform.OSX,
                    new Dictionary<Architecture,string>()
                    {
                        {Architecture.X64,"https://download.visualstudio.microsoft.com/download/pr/3de2d949-fcb5-4586-a217-2c33854d295f/943f0d92252338e11fd11b002a3a3861/dotnet-sdk-5.0.103-osx-x64.tar.gz"},
                    }
                },
                {
                    OSPlatform.Linux,
                    new Dictionary<Architecture,string>()
                    {
                        {Architecture.X64,"https://download.visualstudio.microsoft.com/download/pr/a2052604-de46-4cd4-8256-9bc222537d32/a798771950904eaf91c0c37c58f516e1/dotnet-sdk-5.0.103-linux-x64.tar.gz"},
                        {Architecture.Arm64,"https://download.visualstudio.microsoft.com/download/pr/5c2e5668-d7f9-4705-acb0-04ceeda6dadf/4eca3d1ffd92cb2b5f9152155a5529b4/dotnet-sdk-5.0.103-linux-arm64.tar.gz"},
                        {Architecture.Arm,"https://download.visualstudio.microsoft.com/download/pr/cd11b0d1-8d79-493f-a702-3ecbadb040aa/d24855458a90944d251dd4c68041d0b7/dotnet-sdk-5.0.103-linux-arm.tar.gz"},
                    }
                },
            };
        
        private string ParentDirectory;
        private string InstallDirectory;
        private string BinaryToDownload;
        
        /*
         * Creates a .NET 5 Prerequisite object.
         */
        public DotNet5(string parentDirectory)
        {
            this.ParentDirectory = parentDirectory;
            this.InstallDirectory = Path.Combine(this.ParentDirectory,"dotnet5");
            
            // Determine the binary to download.
            foreach (var operatingSystems in DOTNET_5_DOWNLOADS)
            {
                if (RuntimeInformation.IsOSPlatform(operatingSystems.Key) && operatingSystems.Value.ContainsKey(RuntimeInformation.OSArchitecture))
                {
                    this.BinaryToDownload = operatingSystems.Value[RuntimeInformation.OSArchitecture];
                    
                }
            }
        }
        
        /*
         * Returns the name of the prerequisite.
         */
        public string GetName()
        {
             return ".NET 5";
        }
        
        /*
         * Returns the error message for the
         * prerequisite not being met.
         */
        public string GetErrorMessage()
        {
            return ".NET 5 is not installed. Due to how installed, seeing this means an error occured.";
        }
        
        /*
         * Handles setting up the prerequisite.
         * Returns if it was completed successfully,
         * and false if it wasn't.
         */
        public bool SetupPrerequisite()
        {
            // Throw an exception for an unsupported platform.
            if (this.BinaryToDownload == null)
            {
                throw new PlatformNotSupportedException(".NET 5 can't be downloaded for your OS and architecture.");
            }
            
            // Set up the parent directory and clean the existing directory.
            Directory.CreateDirectory(this.ParentDirectory);
            if (Directory.Exists(this.InstallDirectory))
            {
                Directory.Delete(this.InstallDirectory,true);
            }
            
            // Download the binary.
            var client = new WebClient();
            var isTarGz = this.BinaryToDownload.EndsWith(".tar.gz");
            string binaryDownloadLocation = null;
            if (isTarGz)
            {
                binaryDownloadLocation = Path.Combine(this.ParentDirectory,"dotnet.tar.gz");
            }
            else
            {
                binaryDownloadLocation = Path.Combine(this.ParentDirectory,"dotnet.zip");
            }
            client.DownloadFile(this.BinaryToDownload,binaryDownloadLocation);
            
            // Extract the directory.
            if (isTarGz)
            {
                using (var fileStream = File.OpenRead(binaryDownloadLocation))
                {
                    using (var gzipStream = new GZipInputStream(fileStream))
                    {
                        var tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                        tarArchive.ExtractContents(this.InstallDirectory);
                        tarArchive.Close();
                    }
                }
            }
            else
            {
                ZipFile.ExtractToDirectory(binaryDownloadLocation,this.InstallDirectory);
            }
            
            // Set the permissions for Unix-based systems.
            var dotNetExecutable = Path.Combine(this.InstallDirectory,"dotnet");
            if (File.Exists(dotNetExecutable) && File.Exists("/bin/bash"))
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        FileName = "/bin/bash",
                        Arguments = "-c \"chmod +x \"" + dotNetExecutable.Replace("\"","\\\"") + "\"\"",
                    }
                };

                process.Start();
                process.WaitForExit();
            }
            
            // Clean up the download file.
            File.Delete(binaryDownloadLocation);
            
            // Return true.
            return true;
        }
        
        /*
         * Returns if the prerequisite was met.
         */
        public bool IsMet()
        {
            // Set up .NET Core 3.0 if it isn't detected.
            if (!Directory.Exists(this.InstallDirectory))
            {
                this.SetupPrerequisite();
            }
            
            // Return true (should install).
            return true;
        }
    }
}