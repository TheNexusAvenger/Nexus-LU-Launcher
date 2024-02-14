using System.Collections.Generic;

namespace Nexus.LU.Launcher.State.Client.Patch.Valve;

public abstract class BaseValveDataFormatEntry
{
    /// <summary>
    /// Name of the entry.
    /// </summary>
    public string Name { get; set; } = null!;
}

public class HeaderValveDataFormatEntry : BaseValveDataFormatEntry
{
    /// <summary>
    /// String value of the entry.
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// Creates an entry.
    /// </summary>
    public HeaderValveDataFormatEntry()
    {
        
    }

    /// <summary>
    /// Creates an entry.
    /// </summary>
    /// <param name="name">Name of the entry.</param>
    /// <param name="value">Value of the entry.</param>
    public HeaderValveDataFormatEntry(string name, string value)
    {
        this.Name = name;
        this.Value = value;
    }
}

public class TextValveDataFormatEntry : BaseValveDataFormatEntry
{
    /// <summary>
    /// 4 bytes at the end of the text.
    /// The 4 bytes may be anything. The docs don't state this.
    /// </summary>
    public byte[] Data { get; set; } = null!;

    /// <summary>
    /// Creates an entry.
    /// </summary>
    public TextValveDataFormatEntry()
    {
        
    }

    /// <summary>
    /// Creates an entry.
    /// </summary>
    /// <param name="name">Name of the entry.</param>
    /// <param name="data">Data of the entry.</param>
    public TextValveDataFormatEntry(string name, byte[] data)
    {
        this.Name = name;
        this.Data = data;
    }

    /// <summary>
    /// Creates an entry with the data as 4 bytes.
    /// </summary>
    /// <param name="name">Name of the entry.</param>
    public TextValveDataFormatEntry(string name) : this(name, new byte[]{0, 0, 0, 0})
    {
        
    }
}

public class SetValveDataFormatEntry<T> : BaseValveDataFormatEntry
{
    /// <summary>
    /// List of values in order part of the set.
    /// </summary>
    public List<KeyValuePair<string, T>> Values { get; set; } = new List<KeyValuePair<string, T>>();
    
    
    /// <summary>
    /// Creates an entry.
    /// </summary>
    public SetValveDataFormatEntry()
    {
        
    }

    /// <summary>
    /// Creates an entry.
    /// </summary>
    /// <param name="name">Name of the entry.</param>
    public SetValveDataFormatEntry(string name)
    {
        this.Name = name;
    }
}

public class ValveDataFormatEntryList : List<BaseValveDataFormatEntry>
{
    /// <summary>
    /// Attempts to return the value for a header name.
    /// </summary>
    /// <param name="header">Header name to find.</param>
    /// <returns>The value of the header, if it exists.</returns>
    public string? TryGetHeader(string header)
    {
        foreach (var entry in this)
        {
            if (entry is not HeaderValveDataFormatEntry headerEntry) continue;
            if (headerEntry.Name != header) continue;
            return headerEntry.Value;
        }
        return null;
    }
}