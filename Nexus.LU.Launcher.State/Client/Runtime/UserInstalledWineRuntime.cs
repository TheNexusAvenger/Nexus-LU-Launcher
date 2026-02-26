using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Client.Patch;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;

namespace Nexus.LU.Launcher.State.Client.Runtime;

public class UserInstalledWineRuntime : IRuntime
{
    /// <summary>
    /// State of the runtime.
    /// </summary>
    public RuntimeState RuntimeState => Environment.GetEnvironmentVariable("PATH")!.Split(":").Any(directory => File.Exists(Path.Combine(directory, "wine"))) ? RuntimeState.Installed : RuntimeState.ManualInstallRequired;
    
    /// <summary>
    /// System info of the client.
    /// </summary>
    private readonly SystemInfo systemInfo;

    /// <summary>
    /// Creates a user installed WINE runtime.
    /// </summary>
    /// <param name="systemInfo">System info of the client.</param>
    public UserInstalledWineRuntime(SystemInfo systemInfo)
    {
        this.systemInfo = systemInfo;
    }
    
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
        // Determine if the Wayland driver should be used.
        var useWayland = false;
        if (this.systemInfo.GetPatchStore("EnableWineWayland", "ForceWaylandDriver")?.ToLower() == "true")
        {
            if (EnableWineWaylandPatch.CanUseWayland())
            {
                Logger.Debug("The WINE Wayland driver will be used.");
                useWayland = true;
            }
            else
            {
                Logger.Warn($"The Wayland driver was requested but {Environment.GetEnvironmentVariable("XDG_SESSION_TYPE")} was detected.");
            }
        }

        // Start the WINE process.
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
        clientProcess.StartInfo.EnvironmentVariables["WINEDLLOVERRIDES"] = "dinput8.dll=n,b";
        if (useWayland)
        {
            // On older WINE installs, this can be required to force starting with the Wayland driver instead of XWayland.
            clientProcess.StartInfo.EnvironmentVariables["DISPLAY"] = "";
        }
        clientProcess.StartInfo.EnvironmentVariables["WINEPREFIX"] = Path.Combine(workingDirectory, "..", "WinePrefix");
        return clientProcess;
    }
}