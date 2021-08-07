using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NLUL.Core.Client.Runtime
{
    public class NativeWindows : IRuntime
    {
        /// <summary>
        /// Name of the runtime.
        /// </summary>
        public string Name => "Native Windows";

        /// <summary>
        /// Whether the emulator is supported on the current platform.
        /// </summary>
        public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        /// <summary>
        /// Whether the emulator can be automatically installed.
        /// </summary>
        public bool CanInstall => false;
        
        /// <summary>
        /// Whether the emulator is installed.
        /// </summary>
        /// <returns></returns>
        public bool IsInstalled => true;

        /// <summary>
        /// The message to display to the user if the runtime
        /// isn't installed and can't be automatically installed.
        /// </summary>
        public string ManualRuntimeInstallMessage => null;
        
        /// <summary>
        /// Attempts to install the emulator.
        /// </summary>
        public void Install()
        {
            throw new NotImplementedException("NativeWindows can be ran without installing.");
        }
        
        /// <summary>
        /// Runs an application in the emulator.
        /// </summary>
        /// <param name="executablePath">Path of the executable to run.</param>
        /// <param name="workingDirectory">Working directory to run the executable in.</param>
        /// <returns>The process of the runtime.</returns>
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