using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nexus.LU.Launcher.State.Client.Archive;

public abstract class ClientArchive
{
    /// <summary>
    /// Types to try for reading archives.
    /// </summary>
    private static readonly List<Func<string, ClientArchive>> ArchiveTypes = new List<Func<string, ClientArchive>>()
    {
        (archiveLocation) => new RarFileArchive(archiveLocation),
        (archiveLocation) => new ZipFileArchive(archiveLocation),
    };
    
    /// <summary>
    /// Event for the archive extraction progressing.
    /// </summary>
    public event Action<float> ExtractProgress;

    /// <summary>
    /// Location of the archive file.
    /// </summary>
    public readonly string ArchiveFile;

    /// <summary>
    /// Required difference since the last ExtractProgress call to raise the event.
    /// </summary>
    public float ReportedProgressBuffer { get; set; }= 0.01f;

    /// <summary>
    /// Last progress that was reported.
    /// </summary>
    private float lastReportedProgress = 0;

    /// <summary>
    /// Creates the client archive.
    /// </summary>
    /// <param name="archiveFile">Location of the archive file.</param>
    public ClientArchive(string archiveFile)
    {
        this.ArchiveFile = archiveFile;
    }

    /// <summary>
    /// Returns the archive for the given file. Returns null if it can't be read or doesn't contain a client.
    /// </summary>
    /// <param name="archiveLocation">Location of the archive.</param>
    /// <returns>The archive for the given file.</returns>
    public static ClientArchive? GetArchive(string archiveLocation)
    {
        return ArchiveTypes.Select(archiveCreator => archiveCreator(archiveLocation)).FirstOrDefault(archive => archive.ContainsClient());
    }

    /// <summary>
    /// Reports progress of extracting.
    /// </summary>
    /// <param name="progress">Progress of the extracting.</param>
    internal void ReportExtractingProgress(float progress)
    {
        // Return if the last reported progress is close.
        // Due to this event invoking UI updates, this can't be passed through every call.
        if (progress != 0 && progress < 1 && Math.Abs(progress - this.lastReportedProgress) < this.ReportedProgressBuffer) return;
        
        // Invoke the event.
        this.lastReportedProgress = progress;
        this.ExtractProgress?.Invoke(progress);
    }
    
    /// <summary>
    /// Determines if an archive contains a client.
    /// </summary>
    /// <returns>Whether the archive contains a client.</returns>
    public abstract bool ContainsClient();

    /// <summary>
    /// Extracts the client in an archive to a directory.
    /// </summary>
    /// <param name="targetLocation">Location to extract to</param>
    public abstract void ExtractTo(string targetLocation);
    
    /// <summary>
    /// Verifies the client in a directory is extracted correctly.
    /// </summary>
    /// <param name="targetLocation">Location to verify.</param>
    /// <returns>Whether the extract was verified.</returns>
    public abstract bool Verify(string targetLocation);
}