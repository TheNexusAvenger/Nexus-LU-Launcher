using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NLUL.Core.Client.Archive
{
    public class ZipFileArchive : ClientArchive
    {
        /// <summary>
        /// Creates the client archive.
        /// </summary>
        /// <param name="archiveFile">Location of the archive file.</param>
        public ZipFileArchive(string archiveFile) : base(archiveFile)
        {
            
        }
        
        /// <summary>
        /// Determines if an archive contains a client.
        /// </summary>
        /// <returns>Whether the archive contains a client.</returns>
        public override bool ContainsClient()
        {
            try
            {
                // Return if a file named legouniverse.exe exists.
                using var zipFile = ZipFile.OpenRead(this.ArchiveFile);
                return zipFile.Entries.Any(entry => entry.Name.ToLower() == "legouniverse.exe");
            }
            catch (Exception)
            {
                // Return false (potentially not a .zip archive).
                return false;
            }
        }

        /// <summary>
        /// Extracts the client in an archive to a directory.
        /// </summary>
        /// <param name="targetLocation">Location to extract to</param>
        public override void ExtractTo(string targetLocation)
        {
            // Get the path of the client.
            using var zipFile = ZipFile.OpenRead(this.ArchiveFile);
            var archiveDirectory = Path.GetDirectoryName(zipFile.Entries.First(entry => entry.Name.ToLower() == "legouniverse.exe").FullName);
            
            // Extract the files.
            var completedFiles = 0;
            this.ReportExtractingProgress(0);
            foreach (var entry in zipFile.Entries)
            {
                // Return if the file is not in the parent directory of legouniverse.exe.
                // Some archives include other files that should not be looked at.
                if (!entry.FullName.ToLower().StartsWith(archiveDirectory!.ToLower()) || entry.FullName.EndsWith("/")) continue;
                
                // Determine the destination file path.
                var filePath = Path.GetRelativePath(archiveDirectory, entry.FullName);
                var newPath = Path.Combine(targetLocation, filePath);
                var newParentPath = Path.GetDirectoryName(newPath);
                
                // Extract the file.
                if (newParentPath != null && !Directory.Exists(newParentPath))
                {
                    Directory.CreateDirectory(newParentPath);
                }
                entry.ExtractToFile(newPath, true);
                
                // Report the progress.
                completedFiles += 1;
                this.ReportExtractingProgress(completedFiles / (float) zipFile.Entries.Count);
            }
            this.ReportExtractingProgress(1);
        }
        
        /// <summary>
        /// Verifies the client in a directory is extracted correctly.
        /// </summary>
        /// <param name="targetLocation">Location to verify.</param>
        /// <returns>Whether the extract was verified.</returns>
        public override bool Verify(string targetLocation)
        {
            // Get the archive entries.
            using var zipFile = ZipFile.OpenRead(this.ArchiveFile);
            var archiveDirectory = Path.GetDirectoryName(zipFile.Entries.First(entry => entry.Name.ToLower() == "legouniverse.exe").FullName);
            
            // Verify the files exist and return false if a file is missing.
            foreach (var entry in zipFile.Entries)
            {
                // Return if the file is not in the parent directory of legouniverse.exe.
                // Some archives include other files that should not be looked at.
                if (!entry.FullName.ToLower().StartsWith(archiveDirectory!.ToLower()) || entry.FullName.EndsWith("/")) continue;
                
                // Determine the destination file path.
                var filePath = Path.Combine(targetLocation, Path.GetRelativePath(archiveDirectory, entry.FullName));
                if (entry.Length == 0 || File.Exists(filePath)) continue;
                return false;
            }

            // Return true (verified).
            return true;
        }
    }
}