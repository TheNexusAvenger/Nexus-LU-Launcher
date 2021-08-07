using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NLUL.Core.Client.Runtime
{
    public class ClientRuntime : IRuntime
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly List<IRuntime> runtimes;
        
        /// <summary>
        /// Creates the client runtime.
        /// </summary>
        /// <param name="systemInfo">Information about the system.</param>
        public ClientRuntime(SystemInfo systemInfo)
        {
            // Create the runtimes.
            // They are in order of how they will be indexed.
            this.runtimes = new List<IRuntime>()
            {
                // Native Windows.
                new NativeWindows(),

                // User-installed WINE.
                new UserInstalledWine(),

                // Automated WINE installs.
                new MacOsWineCrossover(systemInfo),
            };
        }
        
        /// <summary>
        /// Name of the runtime.
        /// </summary>
        public string Name => (from runtime in this.GetSupportedRuntimes() where runtime.CanInstall && !runtime.IsInstalled select runtime.Name).FirstOrDefault();

        /// <summary>
        /// Whether the emulator is supported on the current platform.
        /// </summary>
        public bool IsSupported => this.GetSupportedRuntimes().Count > 0;
        
        /// <summary>
        /// Whether the emulator can be automatically installed.
        /// </summary>
        public bool CanInstall => this.GetSupportedRuntimes().Any(runtime => runtime.CanInstall);
        
        /// <summary>
        /// Whether the emulator is installed.
        /// </summary>
        /// <returns></returns>
        public bool IsInstalled => this.GetSupportedRuntimes().Any(runtime => runtime.IsInstalled);
        
        /// <summary>
        /// The message to display to the user if the runtime
        /// isn't installed and can't be automatically installed.
        /// </summary>
        public string ManualRuntimeInstallMessage => (from runtime in this.GetSupportedRuntimes() where runtime.ManualRuntimeInstallMessage != null select runtime.ManualRuntimeInstallMessage).FirstOrDefault();

        /// <summary>
        /// Returns the runtimes supported for the current platform.
        /// </summary>
        private List<IRuntime> GetSupportedRuntimes()
        {
            return this.runtimes.Where(runtime => runtime.IsSupported).ToList();
        }
        
        /// <summary>
        /// Attempts to install the emulator.
        /// </summary>
        public void Install()
        {
            foreach (var runtime in this.GetSupportedRuntimes().Where(runtime => runtime.CanInstall && !runtime.IsInstalled))
            {
                runtime.Install();
                break;
            }
        }
        
        /// <summary>
        /// Runs an application in the emulator.
        /// </summary>
        /// <param name="executablePath">Path of the executable to run.</param>
        /// <param name="workingDirectory">Working directory to run the executable in.</param>
        /// <returns>The process of the runtime.</returns>
        public Process RunApplication(string executablePath, string workingDirectory)
        {
            foreach (var runtime in this.GetSupportedRuntimes().Where(runtime => runtime.IsInstalled))
            {
                return runtime.RunApplication(executablePath, workingDirectory);
            }

            throw new InvalidOperationException("No valid runtime found.");
        }
    }
}