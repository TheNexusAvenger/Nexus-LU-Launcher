using System.Collections.Generic;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;

namespace Nexus.LU.Launcher.State.Client.Archive;

public class RarFileArchiveEntry : IClientArchiveEntry
{
    /// <summary>
    /// Path of the file.
    /// </summary>
    public string Path => this.entry.Key!;

    /// <summary>
    /// Length of the file for the entry.
    /// </summary>
    public long Length => this.entry.Size;

    /// <summary>
    /// .rar file entry to use.
    /// </summary>
    private readonly IArchiveEntry entry;

    /// <summary>
    /// Creates a .rar archive entry.
    /// </summary>
    /// <param name="entry">.rar file entry to use.</param>
    private RarFileArchiveEntry(IArchiveEntry entry)
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
        var rarFile = RarArchive.OpenArchive(path);
        return rarFile.Entries.Where(entry => !entry.IsDirectory).Select(entry => (IClientArchiveEntry) new RarFileArchiveEntry(entry)).ToList();
    }
    
    /// <summary>
    /// Writes the entry to a path.
    /// </summary>
    /// <param name="path">File of the path.</param>
    public void WriteToFile(string path)
    {
        this.entry.WriteToFile(path);
    }
}