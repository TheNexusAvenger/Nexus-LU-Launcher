/*
 * TheNexusAvenger
 *
 * Downloads, extracts, and installs the client.
 */

using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Client;

namespace NLUL.CLI.Action.Client
{
    public class Install : IAction
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
            return "Downloads, extracts, and patches the client. Writes ~16GB if the ~8GB zip file exists from the download, or ~24GB if it has to be downloaded.";
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
            
            // Install the client.
            var client = new ClientRunner(systemInfo);
            client.TryExtractClient(forceExtract);
            client.PatchClient();
        }
    }
}