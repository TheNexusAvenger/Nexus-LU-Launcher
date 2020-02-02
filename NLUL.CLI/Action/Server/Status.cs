/*
 * TheNexusAvenger
 *
 * Displays the status of the server.
 */

using System;
using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL.CLI.Action.Server
{
    public class Status : IAction
    {
        /*
         * Returns the arguments for the action.
         */
        public string GetArguments()
        {
            return "<serverName>";
        }
        
        /*
         * Returns a description of what the action does.
         */
        public string GetDescription()
        {
            return "Checks the status of the installed server.";
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
                Actions.PrintUsage("server","status");
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
            else
            {
                Console.WriteLine("Server \"" + name + "\"");
                
                // Print if an update is available.
                if (server.IsUpdateAvailable())
                {
                    Console.WriteLine("\tUpdate is available. Use \'server update \"" + name + "\"' to update the server.");
                }
                else
                {
                    Console.WriteLine("\tServer is up to date.");
                }
               
                // Print if the server is running.
                if (server.IsRunning())
                {
                    Console.WriteLine("\tServer is running. Use \'server stop \"" + name + "\"' to stop the server.");
                }
                else
                {
                    Console.WriteLine("\tServer is not running. Use \'server start \"" + name + "\"' to start the server.");
                }
            }
        }
    }
}