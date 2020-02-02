/*
 * TheNexusAvenger
 *
 * Runs the program.
 */
using System;
using NLUL.Core;
using NLUL.Core.Server;

namespace NLUL
{
    class Program
    {
        static void Main(string[] args)
        {
            var systemInfo = new SystemInfo(args[0],args[1]);
            var serverCreator = new ServerCreator(systemInfo);
            var emulator = serverCreator.InitializeServer("UchuDev", ServerType.Uchu);

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
        }
    }
}