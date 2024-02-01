using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nexus.LU.Launcher.State.Model;

public class ServerEntry
{
    /// <summary>
    /// Display name of the server in the launcher.
    /// </summary>
    public string ServerName = null!;
            
    /// <summary>
    /// Server address of the server in the launcher.
    /// </summary>
    public string ServerAddress = null!;
}
    
public class LauncherSettings
{
    /// <summary>
    /// Servers stored in the launcher.
    /// </summary>
    public List<ServerEntry> Servers { get; set; } = new List<ServerEntry>();
            
    /// <summary>
    /// Selected server for the launcher.
    /// </summary>
    public string? SelectedServer { get; set; }

    /// <summary>
    /// Parent location of the clients.
    /// </summary>
    public string? ClientParentLocation { get; set; }
    
    /// <summary>
    /// Whether to display logs after launching.
    /// </summary>
    public bool LogsEnabled { get; set; }
    
    /// <summary>
    /// General storage for tag information.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> PatchStore { get; set; } = new Dictionary<string, Dictionary<string, string>>();
}

[JsonSerializable(typeof(LauncherSettings))]
[JsonSourceGenerationOptions(WriteIndented=true, IncludeFields = true)]
internal partial class LauncherSettingsJsonContext : JsonSerializerContext
{
}

public class SystemInfo
{
    /// <summary>
    /// Static SystemInfo used by different components.
    /// </summary>
    private static SystemInfo _staticSystemInfo = null!;
        
    /// <summary>
    /// Location of configuration file.
    /// </summary>
    private readonly string configurationFileLocation;

    /// <summary>
    /// Location of where clients are stored.
    /// </summary>
    public string SystemFileLocation => this.Settings.ClientParentLocation!;
        
    /// <summary>
    /// Location of the common client that doesn't use a patch server.
    /// </summary>
    public string ClientLocation => Path.Combine(SystemFileLocation, "Client");

    /// <summary>
    /// Settings for the launcher.
    /// </summary>
    public readonly LauncherSettings Settings;
    
    /// <summary>
    /// Creates a Server Info object.
    /// </summary>
    /// <param name="configurationFileLocation">Location of configuration file.</param>
    private SystemInfo(string configurationFileLocation)
    {
        this.configurationFileLocation = configurationFileLocation;
            
        // Load the settings.
        if (File.Exists(this.configurationFileLocation))
        {
            try
            {
                this.Settings = JsonSerializer.Deserialize<LauncherSettings>(File.ReadAllText(this.configurationFileLocation), LauncherSettingsJsonContext.Default.LauncherSettings)!;
            }
            catch (JsonException)
            {
                // Ignore reading the JSON file if it can't be parsed.
                // This probably should be displayed to the user somehow.
            }
        }
        this.Settings ??= new LauncherSettings();
    }
    
    /// <summary>
    /// Returns the value of a key for a patch store.
    /// </summary>
    /// <param name="patchName">Name of the patch to store for.</param>
    /// <param name="key">Key to get.</param>
    /// <returns>Value of the store, if it exists.</returns>
    public string? GetPatchStore(string patchName, string key)
    {
        return !this.Settings.PatchStore.TryGetValue(patchName, out var patchStore) ? null : patchStore.GetValueOrDefault(key);
    }

    /// <summary>
    /// Sets the value of a key in a patch store.
    /// </summary>
    /// <param name="patchName">Name of the patch to store for.</param>
    /// <param name="key">Key to set.</param>
    /// <param name="value">Value to set.</param>
    public void SetPatchStore(string patchName, string key, string? value)
    {
        if (!Settings.PatchStore.TryGetValue(patchName, out var patchStore))
        {
            patchStore = new Dictionary<string, string>();
            this.Settings.PatchStore[patchName] = patchStore;
        }
        if (value == null)
        {
            patchStore.Remove(key);
        }
        else
        {
            patchStore[key] = value;
        }
    }
    
    /// <summary>
    /// Returns the default server info.
    /// </summary>
    private static SystemInfo GetDefaultInstance()
    {
        // Get the base system info.
        var dataDirectory = Environment.GetEnvironmentVariable("XDG_DATA_HOME") ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var baseNlulHome = Path.Combine(dataDirectory, ".nlul");
        var launcherFileLocation = Path.Combine(baseNlulHome, "launcher.json");
        var systemInfo = new SystemInfo(launcherFileLocation);
            
        // Set the default parent directory if none exists.
        if (systemInfo.Settings.ClientParentLocation == null)
        {
            systemInfo.Settings.ClientParentLocation = baseNlulHome;
            systemInfo.SaveSettings();
        }

        // Return the system info.
        return systemInfo;
    }

    /// <summary>
    /// Returns the default server info.
    /// </summary>
    public static SystemInfo GetDefault()
    {
        return _staticSystemInfo ??= GetDefaultInstance();
    }

    /// <summary>
    /// Saves the settings.
    /// </summary>
    public void SaveSettings()
    {
        // Create the directories.
        if (!Directory.Exists(this.SystemFileLocation))
        {
            Directory.CreateDirectory(this.SystemFileLocation);
        }
            
        // Write the state as JSON.
        File.WriteAllText(this.configurationFileLocation, JsonSerializer.Serialize(this.Settings, LauncherSettingsJsonContext.Default.LauncherSettings));
    }
}