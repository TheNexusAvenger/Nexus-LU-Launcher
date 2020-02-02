/*
 * TheNexusAvenger
 *
 * Stores information about a server.
 */

using System;
using NLUL.Core.Server.Emulator;

namespace NLUL.Core.Server
{
    public class ServerInfo
    {
        public readonly SystemInfo SystemInfo;
        public readonly string ServerFileLocation;
        public readonly string ClientLocation;
        public readonly ServerType Type;
        
        /*
         * Creates a Server Info object.
         */
        public ServerInfo(SystemInfo systemInfo,string serverFileLocation,string clientLocation,ServerType type)
        {
            this.SystemInfo = systemInfo;
            this.ServerFileLocation = serverFileLocation;
            this.ClientLocation = clientLocation;
            this.Type = type;
        }
        
        /*
         * Returns an Emulator for the given type.
         */
        public IEmulator CreateEmulator()
        {
            // Return a new emulator.
            if (this.Type == ServerType.Uchu)
            {
                return new UchuServer(this);
            }

            // Return null (no option).
            return null;
        }
    }
}