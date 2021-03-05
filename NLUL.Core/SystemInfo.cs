/*
 * TheNexusAvenger
 *
 * Stores information about the system.
 */

using System;
using System.IO;

namespace NLUL.Core
{
    public class SystemInfo
    {
        public readonly string SystemFileLocation;
        public readonly string ClientLocation;
        
        /*
         * Creates a Server Info object.
         */
        public SystemInfo(string systemFileLocation,string clientLocation)
        {
            this.SystemFileLocation = systemFileLocation;
            this.ClientLocation = clientLocation;
        }
        
        /*
         * Returns the default server info.
         */
        public static SystemInfo GetDefault()
        {
            // Get the custom home if it is defined.
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
                return new SystemInfo(nlulHome,Path.Combine(nlulHome, "Client"));
            }
            
            // Return the default home.
            nlulHome = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nlul");
            return new SystemInfo(nlulHome,Path.Combine(nlulHome, "Client"));
        }
    }
}