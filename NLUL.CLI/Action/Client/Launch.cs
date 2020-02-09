/*
 * TheNexusAvenger
 *
 * Launches the client.
 */

using System;
using System.Collections.Generic;
using System.IO;
using NLUL.Core;
using NLUL.Core.Client;

namespace NLUL.CLI.Action.Client
{
    public class Launch : IAction
    {
        /*
         * Returns the arguments for the action.
         */
        public string GetArguments()
        {
            return "(host)";
        }
        
        /*
         * Returns a description of what the action does.
         */
        public string GetDescription()
        {
            return "Launches the client. If the host is not defined, localhost is used.";
        }
        
        /*
         * Performs the action.
         */
        public void Run(List<string> arguments,SystemInfo systemInfo)
        {
            // Determine the host.
            var host = "localhost";
            if (arguments.Count >= 3)
            {
                host = arguments[2];
            } 
            
            // Return if the client doesn't exist.
            if (!Directory.Exists(systemInfo.ClientLocation) || !File.Exists(Path.Combine(systemInfo.ClientLocation,"legouniverse.exe")))
            {
                Console.WriteLine("Client is not installed. Use \"client install\" or \"client install --force\" to install.");
                return;
            }
            
            // Launch the client.
            var client = new ClientRunner(systemInfo);
            client.Launch(host);
        }
    }
}