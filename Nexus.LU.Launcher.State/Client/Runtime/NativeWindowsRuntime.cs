using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.State.Client.Runtime;

public class NativeWindowsRuntime : IRuntime
{
    /// <summary>
    /// Name of the runtime.
    /// </summary>
    public string Name => "Native Windows";

    /// <summary>
    /// State of the runtime.
    /// </summary>
    public RuntimeState RuntimeState => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Enum.RuntimeState.Installed : RuntimeState.Unsupported;

    /// <summary>
    /// Attempts to install the emulator.
    /// </summary>
    public Task InstallAsync()
    {
        throw new NotImplementedException("NativeWindows can't be installed.");
    }

    /// <summary>
    /// Runs an application in the emulator.
    /// </summary>
    /// <param name="executablePath">Path of the executable to run.</param>
    /// <param name="workingDirectory">Working directory to run the executable in.</param>
    /// <returns>The process of the runtime.</returns>
    public Process RunApplication(string executablePath, string workingDirectory)
    {
        Logger.Info("Starting natively for Windows.");
        return new Process
        {
            StartInfo =
            {
                FileName = executablePath,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            }
        };
    }
}