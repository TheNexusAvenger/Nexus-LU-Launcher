/*
 * TheNexusAvenger
 *
 * Stores the command line actions that can be used.
 */

using System;
using System.Collections.Generic;
using NLUL.CLI.Action.Server;

namespace NLUL.CLI.Action
{
    public class Actions
    {
        public static readonly Dictionary<string,Dictionary<string,IAction>> ACTIONS = new Dictionary<string,Dictionary<string,IAction>>()
        {
            {"server",new Dictionary<string,IAction>()
            {
                {"status", new Status()},
                {"start", new Start()},
                {"stop", new Stop()},
                {"update", new Update()},
                {"new", new New()},
                {"delete", new Delete()},
            }},
        };
        
        /*
         * Prints the help information of an action.
         */
        public static void PrintUsage(string group,string name)
        {
            var action = ACTIONS[group][name];
            Console.WriteLine(group + " " + name + " " + action.GetArguments());
            Console.WriteLine("\t" + action.GetDescription() + "\n");
        }
        
        /*
         * Prints the help information of a group.
         */
        public static void PrintUsage(string group)
        {
            foreach (var actionName in ACTIONS[group].Keys)
            {
                PrintUsage(group,actionName);
            }
        }
        
        /*
         * Prints the help information of all actions.
         */
        public static void PrintUsage()
        {
            foreach (var actionGroup in ACTIONS.Keys)
            {
                PrintUsage(actionGroup);
            }
        }
    }
}