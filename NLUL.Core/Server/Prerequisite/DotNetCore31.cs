/*
 * TheNexusAvenger
 *
 * Prerequisite that requires .NET Core 3.1.
 * Downloads the binaries to a known location.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;

namespace NLUL.Core.Server.Prerequisite
{
    public class DotNetCore31 : IPrerequisite
    {
        public static readonly Dictionary<OSPlatform,Dictionary<Architecture,string>> DOTNET_CORE_3_DOWNLOADS =
            new Dictionary<OSPlatform,Dictionary<Architecture,string>>()
            {
                {
                    OSPlatform.Windows,
                    new Dictionary<Architecture,string>()
                    {
                        {Architecture.X64,"https://download.visualstudio.microsoft.com/download/pr/87955c8d-c571-471a-9d2d-90fd069cf1f2/9fbde37bbe8b156cec97a25b735f9465/dotnet-sdk-3.1.101-win-x64.zip"},
                        {Architecture.X86,"https://download.visualstudio.microsoft.com/download/pr/551b970a-9cb6-418c-9ad9-45fafdab5758/1ba88620682289810c461057c4671bfa/dotnet-sdk-3.1.101-win-x86.zip"},
                        {Architecture.Arm64,"https://download.visualstudio.microsoft.com/download/pr/7363a148-a9e0-4393-b0f6-4e51ecba3e27/4b28aec090c9854d71925bb6d50c8314/dotnet-sdk-3.1.101-win-arm.zip"},
                        {Architecture.Arm,"https://download.visualstudio.microsoft.com/download/pr/7363a148-a9e0-4393-b0f6-4e51ecba3e27/4b28aec090c9854d71925bb6d50c8314/dotnet-sdk-3.1.101-win-arm.zip"},
                    }
                },
                {
                    OSPlatform.OSX,
                    new Dictionary<Architecture,string>()
                    {
                        {Architecture.X64,"https://download.visualstudio.microsoft.com/download/pr/515b77f4-4678-4b6f-a981-c48cf5607c5a/24b33941ba729ec421aa358fa452fd2f/dotnet-sdk-3.1.101-osx-x64.tar.gz"},
                    }
                },
                {
                    OSPlatform.Linux,
                    new Dictionary<Architecture,string>()
                    {
                        {Architecture.X64,"https://download.visualstudio.microsoft.com/download/pr/c4b503d6-2f41-4908-b634-270a0a1dcfca/c5a20e42868a48a2cd1ae27cf038044c/dotnet-sdk-3.1.101-linux-x64.tar.gz"},
                        {Architecture.Arm64,"https://download.visualstudio.microsoft.com/download/pr/cf54dd72-eab1-4f5c-ac1e-55e2a9006739/d66fc7e2d4ee6c709834dd31db23b743/dotnet-sdk-3.1.101-linux-arm64.tar.gz"},
                        {Architecture.Arm,"https://download.visualstudio.microsoft.com/download/pr/d52fa156-1555-41d5-a5eb-234305fbd470/173cddb039d613c8f007c9f74371f8bb/dotnet-sdk-3.1.101-linux-arm.tar.gz"},
                    }
                },
            };
        
        private string ParentDirectory;
        private string InstallDirectory;
        private string BinaryToDownload;
        
        /*
         * Creates a .NET Core 3.1 Prerequisite object.
         */
        public DotNetCore31(string parentDirectory)
        {
            this.ParentDirectory = parentDirectory;
            this.InstallDirectory = Path.Combine(this.ParentDirectory,"dotnet3.1");
            
            // Determine the binary to download.
            foreach (var operatingSystems in DOTNET_CORE_3_DOWNLOADS)
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
             return ".NET Core 3.1";
        }
        
        /*
         * Returns the error message for the
         * prerequisite not being met.
         */
        public string GetErrorMessage()
        {
            return ".NET Core 3.1 is not installed. Due to how installed, seeing this means an error occured.";
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
                throw new PlatformNotSupportedException(".NET Core 3.1 can't be downloaded for your OS and architecture.");
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