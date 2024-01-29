using System;
using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;

namespace Nexus.LU.Launcher.State.Client.Archive;

public class RarFileArchive : ClientArchive
{
    /// <summary>
    /// Creates the client archive.
    /// </summary>
    /// <param name="archiveFile">Location of the archive file.</param>
    public RarFileArchive(string archiveFile) : base(archiveFile)
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
            using var rarFile = RarArchive.Open(this.ArchiveFile);
            return rarFile.Entries.Any(entry => Path.GetFileName(entry.Key).ToLower() == "legouniverse.exe");
        }
        catch (Exception)
        {
            // Return false (potentially not a .rar archive).
            Logger.Warn($"File \"{this.ArchiveFile}\" is not a RAR file.");
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
        using var rarFile = RarArchive.Open(this.ArchiveFile);
        var archiveDirectory = Path.GetDirectoryName(rarFile.Entries.First(entry => Path.GetFileName(entry.Key).ToLower() == "legouniverse.exe").Key);
        
        // Extract the files.
        var completedFiles = 0;
        this.ReportExtractingProgress(0);
        foreach (var entry in rarFile.Entries)
        {
            // Return if the file is not in the parent directory of legouniverse.exe.
            // Some archives include other files that should not be looked at.
            if (!entry.Key.ToLower().StartsWith(archiveDirectory!.ToLower()) || entry.IsDirectory) continue;
            
            // Determine the destination file path.
            var filePath = archiveDirectory == "" ? entry.Key : Path.GetRelativePath(archiveDirectory, entry.Key);
            var newPath = Path.Combine(targetLocation, filePath);
            var newParentPath = Path.GetDirectoryName(newPath);
            
            // Extract the file.
            if (newParentPath != null && !Directory.Exists(newParentPath))
            {
                Directory.CreateDirectory(newParentPath);
            }
            entry.WriteToFile(newPath);
            Logger.Debug($"Extracted file {newPath}");
            
            // Report the progress.
            completedFiles += 1;
            this.ReportExtractingProgress(completedFiles / (float) rarFile.Entries.Count);
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
        using var rarFile = RarArchive.Open(this.ArchiveFile);
        var archiveDirectory = Path.GetDirectoryName(rarFile.Entries.First(entry => Path.GetFileName(entry.Key).ToLower() == "legouniverse.exe").Key);
        
        // Verify the files exist and return false if a file is missing.
        foreach (var entry in rarFile.Entries)
        {
            // Return if the file is not in the parent directory of legouniverse.exe.
            // Some archives include other files that should not be looked at.
            if (!entry.Key.ToLower().StartsWith(archiveDirectory!.ToLower()) || entry.IsDirectory) continue;
            
            // Determine the destination file path.
            var filePath = archiveDirectory == "" ? entry.Key : Path.GetRelativePath(archiveDirectory, entry.Key);
            var newPath = Path.Combine(targetLocation, filePath);
            if (entry.Size == 0 || File.Exists(newPath)) continue;
            Logger.Warn($"File {newPath} was not verified correctly.");
            return false;
        }

        // Return true (verified).
        return true;
    }
}