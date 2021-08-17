using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NLUL.Core
{
    public class ServerEntry
    {
        /// <summary>
        /// Display name of the server in the launcher.
        /// </summary>
        public string serverName;
            
        /// <summary>
        /// Server address of the server in the launcher.
        /// </summary>
        public string serverAddress;
    }
    
    public class LauncherSettings
    {
        /// <summary>
        /// Servers stored in the launcher.
        /// </summary>
        public List<ServerEntry> servers = new List<ServerEntry>();
            
        /// <summary>
        /// Selected server for the launcher.
        /// </summary>
        public string selectedServer;
    }
    
    public class SystemInfo
    {
        /// <summary>
        /// Location of configuration file.
        /// </summary>
        private readonly string configurationFileLocation;
        
        /// <summary>
        /// Location of where clients are stored.
        /// </summary>
        public readonly string SystemFileLocation;
        
        /// <summary>
        /// Location of the client.
        /// </summary>
        public readonly string ClientLocation;

        /// <summary>
        /// Settings for the launcher.
        /// </summary>
        public readonly LauncherSettings Settings;
        
        /// <summary>
        /// Creates a Server Info object.
        /// </summary>
        /// <param name="configurationFileLocation">Location of configuration file.</param>
        /// <param name="systemFileLocation">Location of where clients are stored.</param>
        /// <param name="clientLocation">Location of the client.</param>
        public SystemInfo(string configurationFileLocation, string systemFileLocation, string clientLocation)
        {
            this.configurationFileLocation = configurationFileLocation;
            this.SystemFileLocation = systemFileLocation;
            this.ClientLocation = clientLocation;
            
            // Load the settings.
            if (File.Exists(this.configurationFileLocation))
            {
                try
                {
                    this.Settings = JsonConvert.DeserializeObject<LauncherSettings>(File.ReadAllText(this.configurationFileLocation));
                }
                catch (JsonException)
                {
                        
                }
            }
            this.Settings ??= new LauncherSettings();
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        public void SaveSettings()
        {
            // Create the directories.
            var systemInfo = SystemInfo.GetDefault();
            if (!Directory.Exists(systemInfo.SystemFileLocation))
            {
                Directory.CreateDirectory(systemInfo.SystemFileLocation);
            }
            
            // Write the state as JSON.
            File.WriteAllText(this.configurationFileLocation, JsonConvert.SerializeObject(this.Settings, Formatting.Indented));
        }
        
        /// <summary>
        /// Returns the default server info.
        /// </summary>
        public static SystemInfo GetDefault()
        {
            // Get the custom home if it is defined.
            var baseNlulHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nlul");
            var launcherFileLocation = Path.Combine(baseNlulHome, "launcher.json");
            var nlulHome = Environment.GetEnvironmentVariable("NLULHome");
            if (nlulHome != null)
            {
                // Move the .nlul folder if it exists.
                // In V.0.2.1, a custom home would still have a .nlul directory created in a custom home.
                var nlulDirectoryInHome = Path.Combine(nlulHome, ".nlul");
                if (Directory.Exists(nlulDirectoryInHome))
                {
                    foreach (var filePath in Directory.GetFiles(nlulDirectoryInHome))
                    {
                        var fileName = filePath.Substring(nlulDirectoryInHome.Length + 1);
                        File.Move(filePath, Path.Combine(nlulHome, fileName));
                    }
                    foreach (var directoryPath in Directory.GetDirectories(nlulDirectoryInHome))
                    {
                        var directoryName = directoryPath.Substring(nlulDirectoryInHome.Length + 1);
                        Directory.Move(directoryPath, Path.Combine(nlulHome, directoryName));
                    }
                    Directory.Delete(nlulDirectoryInHome, true);
                }

                // Return the custom home.
                return new SystemInfo(launcherFileLocation, nlulHome,Path.Combine(nlulHome, "Client"));
            }
            
            // Return the default home.
            return new SystemInfo(launcherFileLocation, baseNlulHome,Path.Combine(baseNlulHome, "Client"));
        }
    }
}