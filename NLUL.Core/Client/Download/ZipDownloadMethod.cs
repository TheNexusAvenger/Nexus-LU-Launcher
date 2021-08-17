using System;
using System.IO;
using NLUL.Core.Client.Source;
using System.IO.Compression;

namespace NLUL.Core.Client.Download
{
    public class ZipDownloadMethod : FileDownloadMethod
    {
        /// <summary>
        /// Creates the download method.
        /// </summary>
        /// <param name="systemInfo">Information about the system.</param>
        public ZipDownloadMethod(SystemInfo systemInfo) : base(systemInfo)
        {
            
        }

        /// <summary>
        /// Extracts the downloaded client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        public override void Extract(ClientSourceEntry source)
        {
            // Clean the client.
            if (Directory.Exists(this.ExtractLocation))
            {
                Directory.Delete(this.ExtractLocation, true);
            }
            if (Directory.Exists(this.SystemInfo.ClientLocation))
            {
                Directory.Delete(this.SystemInfo.ClientLocation, true);
            }
            
            // Extract the files.
            if (!Directory.Exists(this.ExtractLocation))
            {
                ZipFile.ExtractToDirectory(this.GetDownloadLocation(source), this.ExtractLocation);
            }
            
            // Move the files.
            var extractedDirectory = this.ExtractLocation;
            if (Directory.GetDirectories(this.ExtractLocation).Length == 1)
            {
                extractedDirectory = Path.Combine(extractedDirectory, Directory.GetDirectories(this.ExtractLocation)[0]);
            }
            Directory.Move(extractedDirectory, this.SystemInfo.ClientLocation);
        }

        /// <summary>
        /// Verifies the extracted client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        public override void Verify(ClientSourceEntry source)
        {
            // Return if the client can't be verified.
            var downloadLocation = this.GetDownloadLocation(source);
            if (!File.Exists(downloadLocation))
            {
                return;
            }
            
            // Verify the files exist and throw an exception if a file is missing.
            using (var zipFile = ZipFile.OpenRead(downloadLocation))
            {
                var entries = zipFile.Entries;
                foreach (var entry in entries)
                {
                    // Get the file name to check.
                    var fileName = entry.FullName;
                    if (fileName.Contains("/"))
                    {
                        var directory = fileName.Substring(0, fileName.IndexOf("/", StringComparison.Ordinal));
                        if (!Directory.Exists(Path.Combine(this.SystemInfo.ClientLocation, directory)))
                        {
                            fileName = fileName.Substring(fileName.IndexOf("/", StringComparison.Ordinal) + 1);
                        }
                    }
                    
                    // Throw an exception if the file is missing.
                    var filePath = Path.Combine(this.SystemInfo.ClientLocation, fileName);
                    if (entry.Length == 0 || File.Exists(filePath)) continue;
                    this.OnDownloadStateChanged("VerifyFailed");
                    return;
                }
            }
            
            // Delete the client archive.
            if (File.Exists(downloadLocation))
            {
                File.Delete(downloadLocation);
            }
        }
    }
}