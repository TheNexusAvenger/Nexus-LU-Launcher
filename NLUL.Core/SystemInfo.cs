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
            var nlulHome = Environment.GetEnvironmentVariable("NLULHome") ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var programData = Path.Combine(nlulHome,".nlul");
            return new SystemInfo(programData,Path.Combine(programData,"Client"));
        }
    }
}