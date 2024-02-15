using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Nexus.LU.Launcher.State.Model;

public class LegoDataDictionary
{
    /// <summary>
    /// Values stored in the LEGO Data Dictionary.
    /// </summary>
    private readonly Dictionary<string, (byte, object)> values = new Dictionary<string, (byte, object)>();

    /// <summary>
    /// Reads a LEGO Data Dictionary from a file's contents.
    /// </summary>
    /// <param name="data">Contents of the file to read.</param>
    /// <returns>Parsed LEGO Data Dictionary.</returns>
    public static LegoDataDictionary FromString(string data)
    {
        // Iterate over the lines and add the values.
        var dataDictionary = new LegoDataDictionary();
        foreach (var line in data.Split(","))
        {
            // Read the key, type of the value, and the value.
            var trimmedLine = line.Trim();
            var keyValueSplit = trimmedLine.Split('=', 2);
            var key = keyValueSplit[0];
            var valueSplit = keyValueSplit[1].Split(':', 2);
            var valueType = valueSplit[0];
            var valueString = valueSplit[1];
            
            // Add the entries.
            dataDictionary.Set(key, byte.Parse(valueType), valueString);
        }
        
        // Return the data dictionary.
        return dataDictionary;
    }

    /// <summary>
    /// Reads a LEGO Data Dictionary from a file.
    /// </summary>
    /// <param name="path">Path of the file to read.</param>
    /// <returns>Parsed LEGO Data Dictionary.</returns>
    public static async Task<LegoDataDictionary> FromFileAsync(string path)
    {
        return FromString((await File.ReadAllTextAsync(path)).Trim());
    }

    /// <summary>
    /// Returns the value for a key.
    /// </summary>
    /// <param name="key">Key to get.</param>
    /// <typeparam name="T">Type of the value to get.</typeparam>
    /// <returns>Value for the key.</returns>
    public T Get<T>(string key)
    {
        return (T) this.values[key].Item2;
    }
    
    /// <summary>
    /// Sets a value to the dictionary.
    /// </summary>
    /// <param name="key">Key to add.</param>
    /// <param name="type">Type of the value to add.</param>
    /// <param name="value">Value to add.</param>
    public void Set(string key, byte type, string value)
    {
        // Convert the value.
        // Only types 0 (string), 1 (int), 5 (uint), and 7 (boolean) exist in boot files.
        if (type == 0)
        {
            // Store the string.
            this.values[key] = (type, value);
        }
        else if (type == 1)
        {
            // Parse and store the int.
            this.values[key] = (type, int.Parse(value));
        }
        else if (type == 5)
        {
            // Parse and store the uint.
            this.values[key] = (type, uint.Parse(value));
        }
        else if (type == 7)
        {
            // Parse and store the boolean.
            if (int.TryParse(value, out var parsedInt))
            {
                this.values[key] = (type, (parsedInt == 1));
            }
            else if (bool.TryParse(value, out var parsedBool))
            {
                this.values[key] = (type, parsedBool);
            }
            else
            {
                throw new FormatException($"Failed to parse {value} as boolean.");
            }
        }
        else
        {
            throw new InvalidOperationException($"Unsupported type: {type}");
        }
    }

    /// <summary>
    /// Adds a value to the dictionary.
    /// </summary>
    /// <param name="key">Key to set.</param>
    /// <param name="value">Value to set.</param>
    public void Set(string key, object value)
    {
        // Determine the type.
        var type = (byte) 0;
        if (value is int)
        {
            type = 1;
        }
        else if (value is uint)
        {
            type = 5;
        }
        else if (value is bool)
        {
            type = 7;
        }
        else if (value is not string)
        {
            throw new InvalidOperationException($"Unsupported type: {value.GetType()}");
        }
        
        // Store the value.
        this.values[key] = (type, value);
    }

    /// <summary>
    /// Converts the LEGO data dictionary back to a string.
    /// </summary>
    /// <returns>String version of the data dictionary.</returns>
    public override string ToString()
    {
        var lines = new List<string>();
        foreach (var (key, value) in this.values)
        {
            var writeValue = value.Item2;
            if (value.Item1 == 7)
            {
                writeValue = (((bool) value.Item2) ? 1 : 0);
            }
            lines.Add($"{key}={value.Item1}:{writeValue}");
        }
        return string.Join(",\n", lines);
    }
}