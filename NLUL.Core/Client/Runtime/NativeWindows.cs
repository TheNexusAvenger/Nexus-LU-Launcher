/*
 * TheNexusAvenger
 *
 * Runtime for Windows.
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NLUL.Core.Client.Runtime
{
    public class NativeWindows : IRuntime
    {
        /*
         * Returns if the emulator is supported on the current platform.
         */
        public bool IsSupported()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
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
            return true;
        }
        
        /*
         * Returns the message to display to the user if the runtime
         * isn't installed and can't be automatically installed.
         */
        public string GetManualRuntimeInstallMessage()
        {
            return null;
        }
        
        /*
         * Attempts to install the emulator.
         */
        public void Install()
        {
            throw new NotImplementedException("NativeWindows can be ran without installing.");
        }
        
        /*
         * Runs an application in the emulator.
         */
        public Process RunApplication(string executablePath, string workingDirectory)
        {
            return new Process
            {
                StartInfo =
                {
                    FileName = executablePath,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                }
            };
        }
    }
}