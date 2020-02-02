/*
 * TheNexusAvenger
 *
 * Creates a server.
 */

using System.IO;
using NLUL.Core.Server.Emulator;

namespace NLUL.Core.Server
{
    public class ServerCreator
    {
        private SystemInfo SystemInfo;
        
        /*
         * Creates a Server Creator object.
         */
        public ServerCreator(SystemInfo systemInfo)
        {
            this.SystemInfo = systemInfo;
        }
        
        /*
         * Initializes a server.
         */
        public IEmulator InitializeServer(string name,ServerType type)
        {
            // Create the server info.
            // TODO: Check for name conflicts
            // TODO: Escape names
            var serverDirectory = Path.Combine(this.SystemInfo.SystemFileLocation, "Server",name);
            Directory.CreateDirectory(serverDirectory);
            var serverInfo = new ServerInfo(this.SystemInfo,serverDirectory,this.SystemInfo.ClientLocation,type);
            
            // Return the server emulator.
            return serverInfo.CreateEmulator();
        }
    }
}