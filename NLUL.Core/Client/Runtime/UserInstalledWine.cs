using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NLUL.Core.Client.Runtime
{
    public class UserInstalledWine : IRuntime
    {
        /// <summary>
        /// Name of the runtime.
        /// </summary>
        public string Name => "WINE";

        /// <summary>
        /// Whether the emulator is supported on the current platform.
        /// </summary>
        public bool IsSupported => true;

        /// <summary>
        /// Whether the emulator can be automatically installed.
        /// </summary>
        public bool CanInstall => false;
        
        /// <summary>
        /// Whether the emulator is installed.
        /// </summary>
        /// <returns></returns>
        public bool IsInstalled => Environment.GetEnvironmentVariable("PATH").Split(":").Any(directory => File.Exists(Path.Combine(directory, "wine")));

        /// <summary>
        /// The message to display to the user if the runtime
        /// isn't installed and can't be automatically installed.
        /// </summary>
        public string ManualRuntimeInstallMessage => "WINE must be installed.";

        /// <summary>
        /// Attempts to install the emulator.
        /// </summary>
        public void Install()
        {
            throw new NotImplementedException("WINE must be installed.");
        }
        
        /// <summary>
        /// Runs an application in the emulator.
        /// </summary>
        /// <param name="executablePath">Path of the executable to run.</param>
        /// <param name="workingDirectory">Working directory to run the executable in.</param>
        /// <returns>The process of the runtime.</returns>
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