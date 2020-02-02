/*
 * TheNexusAvenger
 *
 * Stops a server.
 */

using System;
using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL.Action.Server
{
    public class Stop : IAction
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
            return "Stop a server that currently is running.";
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
            else if (!server.IsRunning())
            {
                Console.WriteLine("Server \"" + name + "\" isn't running. Use \'server start \"" + name + "\"' to start the server.");
            }
            else
            {
                Console.WriteLine("Stopping server \"" + name + "\".");
                server.Stop();
                Console.WriteLine("Stopped server \"" + name + "\".");
            }
        }
    }
}