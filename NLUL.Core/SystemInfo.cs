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
        public string ServerName;
            
        /// <summary>
        /// Server address of the server in the launcher.
        /// </summary>
        public string ServerAddress;
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
        public string SelectedServer { get; set; }
        
        /// <summary>
        /// Parent location of the clients.
        /// </summary>
        public string ClientParentLocation { get; set; }
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
        public string SystemFileLocation => this.Settings.ClientParentLocation;
        
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
        public SystemInfo(string configurationFileLocation)
        {
            this.configurationFileLocation = configurationFileLocation;
            
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
            if (!Directory.Exists(this.SystemFileLocation))
            {
                Directory.CreateDirectory(this.SystemFileLocation);
            }
            
            // Write the state as JSON.
            File.WriteAllText(this.configurationFileLocation, JsonConvert.SerializeObject(this.Settings, Formatting.Indented));
        }
        
        /// <summary>
        /// Returns the default server info.
        /// </summary>
        public static SystemInfo GetDefault()
        {
            // Get the base system info.
            var baseNlulHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nlul");
            var launcherFileLocation = Path.Combine(baseNlulHome, "launcher.json");
            var systemInfo = new SystemInfo(launcherFileLocation);
            
            // Set the default parent directory if none exists.
            if (systemInfo.Settings.ClientParentLocation == null)
            {
                // NLULHome was added in V.0.2.1 and deprecated in V.0.3.0.
                var nlulHome = Environment.GetEnvironmentVariable("NLULHome");
                if (nlulHome != null)
                {
                    systemInfo.Settings.ClientParentLocation = nlulHome;
                    if (!Directory.Exists(nlulHome))
                    {
                        Directory.CreateDirectory(nlulHome);
                    }
                }
                else
                {
                    systemInfo.Settings.ClientParentLocation = baseNlulHome;
                }
                systemInfo.SaveSettings();
            }

            // Return the system info.
            return systemInfo;
        }
    }
}