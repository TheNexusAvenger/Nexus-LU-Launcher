/*
 * TheNexusAvenger
 *
 * Extracted the client.
 */

using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Client;

namespace NLUL.CLI.Action.Client
{
    public class Extract : IAction
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
            return "Downloads and extracts an unpacked client as a .zip file. Writes ~16GB if the ~8GB zip file exists from the download, or ~24GB if it has to be downloaded.";
        }
        
        /*
         * Performs the action.
         */
        public void Run(List<string> arguments,SystemInfo systemInfo)
        {
            // Determine if the action is forced.
            var forceExtract = false;
            if (arguments.Count >= 3 && arguments[2].ToLower() == "--force")
            {
                forceExtract = true;
            } 
            
            // Extract the client.
            var client = new ClientRunner(systemInfo);
            client.TryExtractClient(forceExtract);
        }
    }
}