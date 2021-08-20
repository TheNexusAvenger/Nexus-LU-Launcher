using System;
using System.IO;
using NLUL.Core.Client.Source;
using System.IO.Compression;

namespace NLUL.Core.Client.Download
{
    public class ZipDownloadMethod : FileDownloadMethod
    {
        /// <summary>
        /// Whether the extracted client can be verified.
        /// </summary>
        public override bool CanVerifyExtractedClient => File.Exists(this.DownloadLocation);
        
        /// <summary>
        /// Creates the download method.
        /// </summary>
        /// <param name="systemInfo">Information about the system.</param>
        public ZipDownloadMethod(SystemInfo systemInfo, ClientSourceEntry source) : base(systemInfo, source)
        {
            
        }

        /// <summary>
        /// Extracts the downloaded client.
        /// </summary>
        public override void Extract()
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
                ZipFile.ExtractToDirectory(this.DownloadLocation, this.ExtractLocation);
            }
            
            // Move the files.
            var extractedDirectory = this.ExtractLocation;
            if (Directory.GetDirectories(this.ExtractLocation).Length == 1)
            {
                extractedDirectory = Path.Combine(extractedDirectory, Directory.GetDirectories(this.ExtractLocation)[0]);
            }
            Directory.Move(extractedDirectory, this.SystemInfo.ClientLocation);
            if (Directory.Exists(this.ExtractLocation))
            {
                Directory.Delete(this.ExtractLocation, true);
            }
        }
        
        /// <summary>
        /// Verifies the extracted client.
        /// </summary>
        /// <returns>Whether the client was verified.</returns>
        public override bool Verify()
        {
            // Return if the client can't be verified.
            if (!File.Exists(this.DownloadLocation))
            {
                return true;
            }

            try
            {
                // Verify the files exist and throw an exception if a file is missing.
                using (var zipFile = ZipFile.OpenRead(this.DownloadLocation))
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
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                // Clear the archive and return false. This would have happened if the archive is corrupted.
                if (File.Exists(this.DownloadLocation))
                {
                    File.Delete(this.DownloadLocation);
                }
                return false;
            }
            
            
            // Delete the client archive.
            if (File.Exists(this.DownloadLocation))
            {
                File.Delete(this.DownloadLocation);
            }
            return true;
        }
    }
}