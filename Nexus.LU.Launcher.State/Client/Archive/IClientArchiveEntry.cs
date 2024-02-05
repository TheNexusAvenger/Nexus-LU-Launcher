namespace Nexus.LU.Launcher.State.Client.Archive;

public interface IClientArchiveEntry
{
    /// <summary>
    /// Path of the file.
    /// </summary>
    public string Path { get; }
    
    /// <summary>
    /// Length of the file for the entry.
    /// </summary>
    public long Length { get; }

    /// <summary>
    /// Writes the entry to a path.
    /// </summary>
    /// <param name="path">File of the path.</param>
    public void WriteToFile(string path);
}