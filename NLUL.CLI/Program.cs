/*
 * TheNexusAvenger
 *
 * Runs the program.
 */

using System;
using System.Collections.Generic;
using System.IO;
using NLUL.Action;
using NLUL.Core;

namespace NLUL
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print the usage information if the arguments are invalid.
            if (args.Length < 1)
            {
                Actions.PrintUsage();
                return;
            }
            if (!Actions.ACTIONS.ContainsKey(args[0].ToLower()))
            {
                Console.WriteLine("Unknown command group \"" + args[0] + "\".");
                Actions.PrintUsage();
                return;
            }
            if (args.Length < 2)
            {
                Actions.PrintUsage();
                return;
            }
            if (!Actions.ACTIONS[args[0].ToLower()].ContainsKey(args[1].ToLower()))
            {
                Console.WriteLine("Unknown command \"" + args[1] + "\" in group \"" + args[0] + "\".");
                Actions.PrintUsage(args[0].ToLower());
                return;
            }
            
            // Run the action.
            var programData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".nlul");
            var systemInfo = new SystemInfo(programData,Path.Combine(programData,"Client"));
            Actions.ACTIONS[args[0].ToLower()][args[1]].Run(new List<string>(args),systemInfo);
        }
    }
}