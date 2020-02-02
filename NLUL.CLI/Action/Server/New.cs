/*
 * TheNexusAvenger
 *
 * Creates a new server.
 */

using System;
using System.Collections.Generic;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL.Action.Server
{
    public class New : IAction
    {
        /*
         * Returns the arguments for the action.
         */
        public string GetArguments()
        {
            return "<serverType> <serverName>";
        }
        
        /*
         * Returns a description of what the action does.
         */
        public string GetDescription()
        {
            return "Creates a new server.\n\tSupports the following types: Uchu";
        }
        
        /*
         * Performs the action.
         */
        public void Run(List<string> arguments,SystemInfo systemInfo)
        {
            // Get the type and name.
            if (arguments.Count < 3)
            {
                Console.WriteLine("serverType not specified.");
                Actions.PrintUsage("server","new");
                return;
            }
            if (arguments.Count < 4)
            {
                Console.WriteLine("serverName not specified.");
                Actions.PrintUsage("server","new");
                return;
            }
            var typeString = arguments[2].ToLower();
            var name = arguments[3];
            
            // Get the server type.
            ServerType type;
            if (typeString == "uchu")
            {
                type = ServerType.Uchu;
            }
            else
            {
                Console.WriteLine("Server type \"" + typeString + "\" not supported.");
                Actions.PrintUsage("server","new");
                return;
            }
            
            // Return if the server already exists.
            var serverCreator = new ServerCreator(systemInfo);
            if (serverCreator.GetServer(name) != null)
            {
                Console.WriteLine("Server \"" + name + "\" already exists.");
                return;
            }
            
            // Check the prerequisites.
            Console.WriteLine("Checking prerequisites for " + type + " server.");
            var server = serverCreator.CreateServer(name,type);
            foreach (var prerequisite in server.GetPrerequisites())
            {
                Console.WriteLine("Checking " + prerequisite.GetName());
                if (!prerequisite.IsMet())
                {
                    Console.WriteLine("Prerequisite not met: " + prerequisite.GetName());
                    Console.WriteLine(prerequisite.GetErrorMessage());
                    return;
                }
            }
            
            // Create the server.
            Console.WriteLine("Creating server \"" + name + "\".");
            server.Install();
            Console.WriteLine("Created server \"" + name + "\".");
        }
    }
}