using System;
using System.Collections.Generic;
using System.IO;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.State.Client.Patch.Valve;

public class ValveDataFormatShortcutsFile : SetValveDataFormatEntry<ValveDataFormatEntryList>
{
    /// <summary>
    /// Byte for a start of set character.
    /// </summary>
    public const byte StartOfSetCharacter = 0;
    
    /// <summary>
    /// Byte for an end of set character.
    /// </summary>
    public const byte EndOfSetCharacter = 8;
    
    /// <summary>
    /// Byte for a start of header character.
    /// </summary>
    public const byte StartOfHeaderCharacter = 1;
    
    /// <summary>
    /// Byte for a start of text character.
    /// </summary>
    public const byte StartOfTextCharacter = 2;
    
    /// <summary>
    /// Reads a VDF file for shortcuts.
    /// Specification: https://developer.valvesoftware.com/wiki/Add_Non-Steam_Game
    /// </summary>
    /// <param name="path">Path of the shortcuts file to read.</param>
    /// <returns>Parsed shortcut file contents.</returns>
    public static ValveDataFormatShortcutsFile FromFile(string path)
    {
        // Open the file stream.
        var stream = File.OpenRead(path);
        
        // Create the initial entry.
        stream.AssertNextByte(StartOfSetCharacter);
        var shortcutsFile = new ValveDataFormatShortcutsFile()
        {
            Name = stream.ReadNullTerminatedString(),
        };
        
        // Read the set until the end.
        while (stream.ReadByte() == StartOfSetCharacter)
        {
            var entry = new ValveDataFormatEntryList();
            shortcutsFile.Values.Add(new KeyValuePair<string, ValveDataFormatEntryList>(stream.ReadNullTerminatedString(), entry));
        
            // Read the entries of the shortcut.
            while (true)
            {
                var readType = stream.ReadByte();
                if (readType == StartOfHeaderCharacter)
                {
                    // Read and store the header (key-value pair).
                    entry.Add(new HeaderValveDataFormatEntry()
                    {
                        Name = stream.ReadNullTerminatedString(),
                        Value = stream.ReadNullTerminatedString(),
                    });
                }
                else if (readType == StartOfTextCharacter)
                {
                    // Read and store the text (value + 4 bytes).
                    entry.Add(new TextValveDataFormatEntry()
                    {
                        Name = stream.ReadNullTerminatedString(),
                        Data = new byte[] {(byte) stream.ReadByte(), (byte) stream.ReadByte(), (byte) stream.ReadByte(), (byte) stream.ReadByte()},
                    });
                }
                else if (readType == StartOfSetCharacter)
                {
                    // Read and store the set.
                    var entrySet = new SetValveDataFormatEntry<string>
                    {
                        Name = stream.ReadNullTerminatedString(),
                    };
                    entry.Add(entrySet);
                    while (stream.ReadByte() == StartOfHeaderCharacter)
                    {
                        entrySet.Values.Add(new KeyValuePair<string, string>(stream.ReadNullTerminatedString(), stream.ReadNullTerminatedString()));
                    }
                }
                else if (readType == EndOfSetCharacter)
                {
                    // Break if there are no more properties.
                    break;
                }
                else
                {
                    // Throw an exception if the current byte is unexpected.
                    throw new InvalidDataException($"Unsupported identifier byte: {readType}");
                }
            }
        }
        
        // For some reason, there is an extra end of set character at the end.
        // This is checked to ensure the reader doesn't break the files of the user.
        stream.AssertNextByte(EndOfSetCharacter); 
        
        // Return the shortcuts file.
        stream.Close();
        return shortcutsFile;
    }

    /// <summary>
    /// Writes the VDF shortcuts file.
    /// Specification: https://developer.valvesoftware.com/wiki/Add_Non-Steam_Game
    /// </summary>
    /// <param name="path">Path to write to.</param>
    public void Write(string path)
    {
        // Open the file stream.
        var stream = File.OpenWrite(path);
        
        // Write the name of the shortcut set.
        stream.WriteByte(StartOfSetCharacter);
        stream.WriteNullTerminatedString(this.Name);
        
        // Write the set.
        foreach (var (name, entries) in this.Values)
        {
            // Write the set entry name.
            stream.WriteByte(StartOfSetCharacter);
            stream.WriteNullTerminatedString(name);
            
            // Write the entries.
            foreach (var entry in entries)
            {
                if (entry is HeaderValveDataFormatEntry headerEntry)
                {
                    // Write the header and value.
                    stream.WriteByte(StartOfHeaderCharacter);
                    stream.WriteNullTerminatedString(headerEntry.Name);
                    stream.WriteNullTerminatedString(headerEntry.Value);
                }
                else if (entry is TextValveDataFormatEntry textEntry)
                {
                    // Write the text and remaining bytes.
                    stream.WriteByte(StartOfTextCharacter);
                    stream.WriteNullTerminatedString(textEntry.Name);
                    for (var i = 0; i < 4; i++)
                    {
                        stream.WriteByte(textEntry.Data[i]);
                    }
                }
                else if (entry is SetValveDataFormatEntry<string> setEntry)
                {
                    // Write the set.
                    stream.WriteByte(StartOfSetCharacter);
                    stream.WriteNullTerminatedString(setEntry.Name);
                    foreach (var (setName, setValue) in setEntry.Values)
                    {
                        stream.WriteByte(StartOfHeaderCharacter);
                        stream.WriteNullTerminatedString(setName);
                        stream.WriteNullTerminatedString(setValue);
                    }
                    stream.WriteByte(EndOfSetCharacter);
                }
            }
            
            // End the set.
            stream.WriteByte(EndOfSetCharacter);
        }
        
        // Complete the set.
        stream.WriteByte(EndOfSetCharacter);
        
        // Write the extra end of set character.
        // This isn't document, but is in the current files.
        stream.WriteByte(EndOfSetCharacter);
        
        // Close the file.
        stream.Flush();
        stream.Close();
    }

    /// <summary>
    /// Adds a entry to the file.
    /// </summary>
    /// <param name="entry">Entry to add.</param>
    public void AddEntry(ValveDataFormatEntryList entry)
    {
        // Determine the highest existing entry.
        var maxEntry = 0;
        foreach (var (key, _) in this.Values)
        {
            if (!int.TryParse(key, out var numberKey)) continue;
            maxEntry = Math.Max(maxEntry, numberKey);
        }
        
        // Add the entry.
        this.Values.Add(new KeyValuePair<string, ValveDataFormatEntryList>((maxEntry + 1).ToString(), entry));
    }
}