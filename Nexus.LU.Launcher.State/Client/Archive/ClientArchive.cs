using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Archive;

public class ClientArchive
{
    /// <summary>
    /// Required difference since the last ExtractProgress call to raise the event.
    /// </summary>
    public const float ReportedProgressBuffer = 0.001f;
    
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
    /// Path of the archive.
    /// </summary>
    private readonly string archiveLocation;

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
    /// <param name="archiveLocation">Path of the archive.</param>
    /// <param name="entries">Entries to write.</param>
    private ClientArchive(string archiveLocation, Dictionary<string, IClientArchiveEntry> entries)
    {
        this.archiveLocation = archiveLocation;
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
                return new ClientArchive(archiveLocation, filteredEntries);
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
        // Split the entries to extract in a round-robin method.
        var totalExtractThreads = Math.Max(1, SystemInfo.GetDefault().Settings.ExtractThreads);
        var rawEntries = new List<Dictionary<string, IClientArchiveEntry>>() { this.entries };
        var roundRobinEntries = new List<Dictionary<string, IClientArchiveEntry>>() { new Dictionary<string, IClientArchiveEntry>() };
        for (var i = 1; i < totalExtractThreads; i++)
        {
            rawEntries.Add(GetArchive(this.archiveLocation)!.entries);
            roundRobinEntries.Add(new Dictionary<string, IClientArchiveEntry>());
        }
        var currentThreadIndex = 0;
        foreach (var (relativePath, _) in this.entries)
        {
            roundRobinEntries[currentThreadIndex][relativePath] = rawEntries[currentThreadIndex][relativePath];
            currentThreadIndex = (currentThreadIndex + 1) % totalExtractThreads;
        }
        
        // Extract the files.
        float totalFiles = this.entries.Count;
        float completedFiles = 0;
        var reportProgressLock = new SemaphoreSlim(1);
        var extractTasks = new List<Task>();
        this.ReportExtractingProgress(0);
        foreach (var threadEntries in roundRobinEntries)
        {
            extractTasks.Add(Task.Run(() =>
            {
                foreach (var (relativePath, entry) in threadEntries)
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
                    reportProgressLock.Wait();
                    completedFiles += 1;
                    this.ReportExtractingProgress(completedFiles / totalFiles);
                    reportProgressLock.Release();
                }
            }));
        }
        
        // Wait for the tasks to complete.
        foreach (var task in extractTasks)
        {
            task.Wait();
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