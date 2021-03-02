/*
 * TheNexusAvenger
 *
 * Collection of the runtimes for the client.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NLUL.Core.Client.Runtime
{
    public class ClientRuntime : IRuntime
    {
        private List<IRuntime> runtimes;
        
        /*
         * Creates the client runtime.
         */
        public ClientRuntime(SystemInfo systemInfo)
        {
            // Create the runtimes.
            // They are in order of how they will be indexed.
            this.runtimes = new List<IRuntime>()
            {
                // Native Windows.
                // new NativeWindows(),

                // User-installed WINE.
                new UserInstalledWine(),

                // Automated WINE installs.
                // TODO
            };
        }
        
        /*
         * Returns the runtimes supported for the current platform.
         */
        private List<IRuntime> GetSupportedRuntimes()
        {
            return this.runtimes.Where(runtime => runtime.IsSupported()).ToList();
        }
        
        /*
         * Returns if the emulator is supported on the current platform.
         */
        public bool IsSupported()
        {
            return this.GetSupportedRuntimes().Count > 0;
        }
        
        /*
         * Returns if the emulator can be automatically installed.
         */
        public bool CanInstall()
        {
            return this.GetSupportedRuntimes().Any(runtime => runtime.CanInstall());
        }
        
        /*
         * Returns if the emulator is installed.
         */
        public bool IsInstalled()
        {
            return this.GetSupportedRuntimes().Any(runtime => runtime.IsInstalled());
        }
        
        /*
         * Returns the message to display to the user if the runtime
         * isn't installed and can't be automatically installed.
         */
        public string GetManualRuntimeInstallMessage()
        {
            return (from runtime in this.GetSupportedRuntimes() where runtime.GetManualRuntimeInstallMessage() != null select runtime.GetManualRuntimeInstallMessage()).FirstOrDefault();
        }
        
        /*
         * Attempts to install the emulator.
         */
        public void Install()
        {
            foreach (var runtime in this.GetSupportedRuntimes())
            {
                if (!runtime.CanInstall() || runtime.IsInstalled()) continue;
                runtime.Install();
                break;
            }
        }
        
        /*
         * Runs an application in the emulator.
         */
        public Process RunApplication(string executablePath, string workingDirectory)
        {
            foreach (var runtime in this.GetSupportedRuntimes())
            {
                if (!runtime.IsInstalled()) continue;
                return runtime.RunApplication(executablePath, workingDirectory);
            }

            throw new InvalidOperationException("No valid runtime found.");
        }
    }
}