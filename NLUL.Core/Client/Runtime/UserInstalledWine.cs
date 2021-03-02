/*
 * TheNexusAvenger
 *
 * Runtime for a user who has installed WINE.
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NLUL.Core.Client.Runtime
{
    public class UserInstalledWine : IRuntime
    {
        /*
         * Returns if the emulator is supported on the current platform.
         */
        public bool IsSupported()
        {
            // WINE should not be installed on Windows, but
            // it isn't checked since NativeWindows should
            // always be first.
            return true;
        }
        
        /*
         * Returns if the emulator can be automatically installed.
         */
        public bool CanInstall()
        {
            return false;
        }
        
        /*
         * Returns if the emulator is installed.
         */
        public bool IsInstalled()
        {
            return Environment.GetEnvironmentVariable("PATH").Split(":").Any(directory => File.Exists(Path.Combine(directory, "wine")));
        }
        
        /*
         * Returns the message to display to the user if the runtime
         * isn't installed and can't be automatically installed.
         */
        public string GetManualRuntimeInstallMessage()
        {
            return "WINE must be installed.";
        }
        
        /*
         * Attempts to install the emulator.
         */
        public void Install()
        {
            throw new NotImplementedException("WINE must be installed.");
        }
        
        /*
         * Runs an application in the emulator.
         */
        public Process RunApplication(string executablePath, string workingDirectory)
        {
            var clientProcess = new Process
            {
                StartInfo =
                {
                    FileName = "wine",
                    Arguments = executablePath,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                }
            };
            clientProcess.StartInfo.EnvironmentVariables.Add("WINEDLLOVERRIDES","dinput8.dll=n,b");
            return clientProcess;
        }
    }
}