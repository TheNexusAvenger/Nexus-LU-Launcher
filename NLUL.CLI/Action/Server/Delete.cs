/*
 * TheNexusAvenger
 *
 * Deletes a server.
 */

using System;
using System.Collections.Generic;
using System.IO;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL.CLI.Action.Server
{
    public class Delete : IAction
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
            return "Deletes a server that currently isn't running.";
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
                Actions.PrintUsage("server","delete");
                return;
            }
            var name = arguments[2];
            
            // Get the server.
            var serverCreator = new ServerCreator(systemInfo);
            var server = serverCreator.GetServer(name);
            
            // Delete the server.
            if (server == null)
            {
                Console.WriteLine("Server \"" + name + "\" does not exist.");
            }
            else if (server.IsRunning())
            {
                Console.WriteLine("Server \"" + name + "\" is already running. Use \'server stop \"" + name + "\"' to stop the server.");
            }
            else
            {
                Console.WriteLine("Deleting server \"" + name + "\".");
                try
                {
                    serverCreator.DeleteServer(name);
                    Console.WriteLine("Deleted server \"" + name + "\".");
                }
                catch (IOException e)
                {
                    Console.WriteLine("Failed to delete files.");
                    if (e.Message.Contains("because it is being used by another process."))
                    {
                        Console.WriteLine("Files are in use by another process.");
                    }
                    else if (e.Message.Contains("is denied."))
                    {
                        Console.WriteLine("Files access was denied. Try restarting the operating system.");
                    }
                    else
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}