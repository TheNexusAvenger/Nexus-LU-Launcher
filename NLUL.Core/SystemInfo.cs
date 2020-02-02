/*
 * TheNexusAvenger
 *
 * Stores information about the system.
 */

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
    }
}