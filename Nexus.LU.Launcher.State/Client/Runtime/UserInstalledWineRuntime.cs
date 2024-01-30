using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.State.Client.Runtime;

public class UserInstalledWineRuntime : IRuntime
{
    /// <summary>
    /// Name of the runtime.
    /// </summary>
    public string Name => "WINE";

    /// <summary>
    /// State of the runtime.
    /// </summary>
    public RuntimeState RuntimeState => Environment.GetEnvironmentVariable("PATH")!.Split(":").Any(directory => File.Exists(Path.Combine(directory, "wine"))) ? Enum.RuntimeState.Installed : RuntimeState.ManualInstallRequired;

    /// <summary>
    /// Attempts to install the emulator.
    /// </summary>
    public Task InstallAsync()
    {
        throw new NotImplementedException("WINE must be manually installed.");
    }

    /// <summary>
    /// Runs an application in the emulator.
    /// </summary>
    /// <param name="executablePath">Path of the executable to run.</param>
    /// <param name="workingDirectory">Working directory to run the executable in.</param>
    /// <returns>The process of the runtime.</returns>
    public Process RunApplication(string executablePath, string workingDirectory)
    {
        Logger.Info("Starting with user-provided WINE install.");
        var clientProcess = new Process
        {
            StartInfo =
            {
                FileName = "wine",
                Arguments = executablePath,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
            }
        };
        clientProcess.StartInfo.EnvironmentVariables.Add("WINEDLLOVERRIDES", "dinput8.dll=n,b");
        clientProcess.StartInfo.EnvironmentVariables.Add("WINEPREFIX", Path.Combine(workingDirectory, "..", "WinePrefix"));
        return clientProcess;
    }
}