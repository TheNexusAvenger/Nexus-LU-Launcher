/*
 * TheNexusAvenger
 *
 * Updates a server.
 */

using System;
using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL.CLI.Action.Server
{
    public class Update : IAction
    {
        /*
         * Returns the arguments for the action.
         */
        public string GetArguments()
        {
            return "<serverName> (--force)";
        }
        
        /*
         * Returns a description of what the action does.
         */
        public string GetDescription()
        {
            return "Updates the server if it isn't running.";
        }
        
        /*
         * Performs the action.
         */
        public void Run(List<string> arguments,SystemInfo systemInfo)
        {
            // Get the name.
            if (arguments.Count < 3)
            {
                Console.WriteLine("serverName not specified.");
                Actions.PrintUsage("server","start");
                return;
            }
            var name = arguments[2];
            
            // Get the server.
            var serverCreator = new ServerCreator(systemInfo);
            var server = serverCreator.GetServer(name);
            
            // Print the status.
            if (server == null)
            {
                Console.WriteLine("Server \"" + name + "\" does not exist.");
            }
            else if (server.IsRunning())
            {
                Console.WriteLine("Server \"" + name + "\" is currently running. Use \'server stop \"" + name + "\"' to stop the server.");
            }
            else if (!server.IsUpdateAvailable() && (arguments.Count < 4 || arguments[3] != "--force"))
            {
                Console.WriteLine("Server \"" + name + "\" is update to date. Use \'server update \"" + name + "\" --force' to force update if desired.");
            } 
            else 
            {
                Console.WriteLine("Updating server \"" + name + "\".");
                server.Install();
                Console.WriteLine("Updated server \"" + name + "\".");
            }
        }
    }
}