using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace Nexus.LU.Launcher.State.Client.Archive;

public class ZipFileArchiveEntry  : IClientArchiveEntry
{
    /// <summary>
    /// Path of the file.
    /// </summary>
    public string Path => this.entry.FullName;

    /// <summary>
    /// Length of the file for the entry.
    /// </summary>
    public long Length => this.entry.Length;

    /// <summary>
    /// .zip file entry to use.
    /// </summary>
    private readonly ZipArchiveEntry entry;

    /// <summary>
    /// Creates a .zip archive entry.
    /// </summary>
    /// <param name="entry">.zip file entry to use.</param>
    private ZipFileArchiveEntry(ZipArchiveEntry entry)
    {
        this.entry = entry;
    }

    /// <summary>
    /// Returns the archive entries for a path.
    /// </summary>
    /// <param name="path">Path of the archive to ready.</param>
    /// <returns>Entries for the archive.</returns>
    public static List<IClientArchiveEntry> GetEntries(string path)
    {
        var zipFile = ZipFile.OpenRead(path);
        return zipFile.Entries.Where(entry => !entry.FullName.EndsWith('/')).Select(entry => (IClientArchiveEntry) new ZipFileArchiveEntry(entry)).ToList();
    }
    
    /// <summary>
    /// Writes the entry to a path.
    /// </summary>
    /// <param name="path">File of the path.</param>
    public void WriteToFile(string path)
    {
        this.entry.ExtractToFile(path);
    }
}