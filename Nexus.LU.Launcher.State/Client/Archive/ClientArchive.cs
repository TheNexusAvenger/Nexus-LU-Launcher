using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nexus.LU.Launcher.State.Client.Archive;

public class ClientArchive
{
    /// <summary>
    /// Required difference since the last ExtractProgress call to raise the event.
    /// </summary>
    public const float ReportedProgressBuffer = 0.01f;
    
    /// <summary>
    /// Methods for getting the archive file entries.
    /// </summary>
    private static readonly Dictionary<string, Func<string, List<IClientArchiveEntry>>> GetEntriesForArchives =
        new Dictionary<string, Func<string, List<IClientArchiveEntry>>>()
        {
            {"RAR", RarFileArchiveEntry.GetEntries},
            {"ZIP", ZipFileArchiveEntry.GetEntries},
        };
    
    /// <summary>
    /// Event for the archive extraction progressing.
    /// </summary>
    public event Action<float>? ExtractProgress;

    /// <summary>
    /// Entries for the archive to write.
    /// </summary>
    private readonly Dictionary<string, IClientArchiveEntry> entries;

    /// <summary>
    /// Last progress that was reported.
    /// </summary>
    private float lastReportedProgress = 0;

    /// <summary>
    /// Creates a client archive.
    /// </summary>
    /// <param name="entries">Entries to write.</param>
    private ClientArchive(Dictionary<string, IClientArchiveEntry> entries)
    {
        this.entries = entries;
    }

    /// <summary>
    /// Returns an archive for the archive file location.
    /// Returns null if the archive is invalid or doesn't contain a client.
    /// </summary>
    /// <param name="archiveLocation">Location of the archive.</param>
    /// <returns>Archive for the client, if it is valid.</returns>
    public static ClientArchive? GetArchive(string archiveLocation)
    {
        foreach (var (archiveType, getEntries) in GetEntriesForArchives)
        {
            try
            {
                // Get the file entries and the entry that contains the executable.
                // An exception is thrown if the archive can't be read.
                // Ignore the archive if there is no executable.
                var entries = getEntries(archiveLocation);
                var clientExecutableEntry = entries.FirstOrDefault(entry => Path.GetFileName(entry.Path).ToLower() == "legouniverse.exe");
                if (clientExecutableEntry == null) continue;
                
                // Get the client files.
                var clientPathDirectory = Path.GetDirectoryName(clientExecutableEntry.Path)!;
                var clientPathDirectoryLower = clientPathDirectory.ToLower();
                var filteredEntries = new Dictionary<string, IClientArchiveEntry>();
                foreach (var entry in entries)
                {
                    // Ignore the entry if it is not for the client or is a directory.
                    if (!entry.Path.ToLower().StartsWith(clientPathDirectoryLower)) continue;

                    // Add the entry.
                    var filePath = clientPathDirectory == "" ? entry.Path : Path.GetRelativePath(clientPathDirectory, entry.Path);
                    filteredEntries[Path.Combine("Client", filePath)] = entry;
                }
                
                // Get the version entries, if the archive includes version files.
                // Some unpacked files may not contain version files, but all packed clients should.
                var versionsTextEntry = entries.FirstOrDefault(entry => Path.GetFileName(entry.Path).ToLower() == "trunk.txt");
                if (versionsTextEntry != null)
                {
                    var versionsPathDirectory = Path.GetDirectoryName(versionsTextEntry.Path)!;
                    var versionsPathDirectoryLower = versionsPathDirectory.ToLower();
                    foreach (var entry in entries)
                    {
                        // Ignore the entry if it is not for the versions.
                        if (!entry.Path.ToLower().StartsWith(versionsPathDirectoryLower)) continue;

                        // Add the entry.
                        var filePath = versionsPathDirectory == "" ? entry.Path : Path.GetRelativePath(versionsPathDirectory, entry.Path);
                        filteredEntries[Path.Combine("versions", filePath)] = entry;
                    }
                }
                
                // Create and return the archive.
                return new ClientArchive(filteredEntries);
            }
            catch (Exception e)
            {
                // Output a warning if the archive type is not correct.
                Logger.Warn($"File \"{archiveLocation}\" is not a {archiveType} file.");
            }
        }
        return null;
    }

    /// <summary>
    /// Reports progress of extracting.
    /// </summary>
    /// <param name="progress">Progress of the extracting.</param>
    private void ReportExtractingProgress(float progress)
    {
        // Return if the last reported progress is close.
        // Due to this event invoking UI updates, this can't be passed through every call.
        if (progress != 0 && progress < 1 && Math.Abs(progress - this.lastReportedProgress) < ReportedProgressBuffer) return;
        
        // Invoke the event.
        this.lastReportedProgress = progress;
        this.ExtractProgress?.Invoke(progress);
    }

    /// <summary>
    /// Extracts the client in an archive to a directory.
    /// </summary>
    /// <param name="targetLocation">Location to extract to</param>
    public void ExtractTo(string targetLocation)
    {
        // Extract the files.
        float totalFiles = this.entries.Count;
        float completedFiles = 0;
        this.ReportExtractingProgress(0);
        foreach (var (relativePath, entry) in this.entries)
        {
            // Determine the destination file path.
            var newPath = Path.Combine(targetLocation, relativePath);
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
            this.ReportExtractingProgress(completedFiles / totalFiles);
        }
    }
    
    /// <summary>
    /// Verifies the client in a directory is extracted correctly.
    /// </summary>
    /// <param name="targetLocation">Location to verify.</param>
    /// <returns>Whether the extract was verified.</returns>
    public bool Verify(string targetLocation)
    {
        // Verify the files exist and return false if a file is missing.
        foreach (var (relativePath, entry) in this.entries)
        {
            // Determine the destination file path.
            var newPath = Path.Combine(targetLocation, relativePath);
            if (entry.Length == 0 || File.Exists(newPath)) continue;
            Logger.Warn($"File {newPath} was not verified correctly.");
            return false;
        }

        // Return true (verified).
        return true;
    }
}