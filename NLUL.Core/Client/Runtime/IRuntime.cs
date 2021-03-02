/*
 * TheNexusAvenger
 *
 * Base runtime for the client.
 */

using System.Diagnostics;

namespace NLUL.Core.Client.Runtime
{
    public interface IRuntime
    {
        /*
         * Returns the name of the runtime.
         */
        public string GetName();
        
        /*
         * Returns if the emulator is supported on the current platform.
         */
        public bool IsSupported();
        
        /*
         * Returns if the emulator can be automatically installed.
         */
        public bool CanInstall();
        
        /*
         * Returns if the emulator is installed.
         */
        public bool IsInstalled();
        
        /*
         * Returns the message to display to the user if the runtime
         * isn't installed and can't be automatically installed.
         */
        public string GetManualRuntimeInstallMessage();
        
        /*
         * Attempts to install the emulator.
         */
        public void Install();
        
        /*
         * Runs an application in the emulator.
         */
        public Process RunApplication(string executablePath, string workingDirectory);
    }
}