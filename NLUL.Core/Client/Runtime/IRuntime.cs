using System.Diagnostics;

namespace NLUL.Core.Client.Runtime
{
    public interface IRuntime
    {
        /// <summary>
        /// Name of the runtime.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Whether the emulator is supported on the current platform.
        /// </summary>
        public bool IsSupported { get; }
        
        /// <summary>
        /// Whether the emulator can be automatically installed.
        /// </summary>
        public bool CanInstall { get; }
        
        /// <summary>
        /// Whether the emulator is installed.
        /// </summary>
        /// <returns></returns>
        public bool IsInstalled { get; }
        
        /// <summary>
        /// The message to display to the user if the runtime
        /// isn't installed and can't be automatically installed.
        /// </summary>
        public string ManualRuntimeInstallMessage { get; }
        
        /// <summary>
        /// Attempts to install the emulator.
        /// </summary>
        public void Install();
        
        /// <summary>
        /// Runs an application in the emulator.
        /// </summary>
        /// <param name="executablePath">Path of the executable to run.</param>
        /// <param name="workingDirectory">Working directory to run the executable in.</param>
        /// <returns>The process of the runtime.</returns>
        public Process RunApplication(string executablePath, string workingDirectory);
    }
}