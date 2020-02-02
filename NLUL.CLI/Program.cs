/*
 * TheNexusAvenger
 *
 * Runs the program.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NLUL.Action;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            var systemInfo = new SystemInfo(args[0],args[1]);
            var serverCreator = new ServerCreator(systemInfo);
            var emulator = serverCreator.CreateServer("UchuDev", ServerType.Uchu);

            foreach (var prerequisite in emulator.GetPrerequisites())
            {
                Console.WriteLine("Checking " + prerequisite.GetName());
                if (!prerequisite.IsMet())
                {
                    Console.WriteLine("Failed: " + prerequisite.GetName());
                    throw new Exception("Failed to install");
                }
            }
            
            emulator.Install();
            emulator.Start();
            Thread.Sleep(30000);
            emulator.Stop();
            */
            
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