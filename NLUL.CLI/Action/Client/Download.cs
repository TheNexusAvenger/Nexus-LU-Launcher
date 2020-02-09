/*
 * TheNexusAvenger
 *
 * Downloads the client without extracting it.
 */

using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Client;

namespace NLUL.CLI.Action.Client
{
    public class Download : IAction
    {
        /*
         * Returns the arguments for the action.
         */
        public string GetArguments()
        {
            return "(--force)";
        }
        
        /*
         * Returns a description of what the action does.
         */
        public string GetDescription()
        {
            return "Downloads an unpacked client as a .zip file. Writes ~8GBs of files.";
        }
        
        /*
         * Performs the action.
         */
        public void Run(List<string> arguments,SystemInfo systemInfo)
        {
            // Determine if the action is forced.
            var forceDownload = false;
            if (arguments.Count >= 3 && arguments[2].ToLower() == "--force")
            {
                forceDownload = true;
            } 
            
            // Download the client.
            var client = new ClientRunner(systemInfo);
            client.DownloadClient(forceDownload);
        }
    }
}