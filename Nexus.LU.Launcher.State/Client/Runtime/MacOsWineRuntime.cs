using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;
using Nexus.LU.Launcher.State.Model;
using Nexus.LU.Launcher.State.Util;

namespace Nexus.LU.Launcher.State.Client.Runtime;

public class MacOsWineRuntime : IRuntime
{
    /// <summary>
    /// WINE download URL.
    /// </summary>
    public const string WineDownloadUrl = "https://github.com/Gcenx/macOS_Wine_builds/releases/download/9.0/wine-stable-9.0-osx64.tar.xz";
    
    /// <summary>
    /// State of the runtime.
    /// </summary>
    public RuntimeState RuntimeState => GetState();

    /// <summary>
    /// Path for WINE to be extracted to.
    /// </summary>
    private string WineExtractPath => Path.Combine(SystemInfo.GetDefault().SystemFileLocation, "Wine");

    /// <summary>
    /// Path for the WINE binary.
    /// </summary>
    private string WinePath => Path.Combine(WineExtractPath, "bin", "wine");

    /// <summary>
    /// Returns the state of the runtime.
    /// </summary>
    /// <returns>State of the runtime.</returns>
    private RuntimeState GetState()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return Enum.RuntimeState.Unsupported;
        return File.Exists(WinePath) ? Enum.RuntimeState.Installed : Enum.RuntimeState.NotInstalled;
    }

    /// <summary>
    /// Attempts to install the emulator.
    /// </summary>
    public async Task InstallAsync()
    {
        // Download WINE using the Homebrew version.
        var downloadPath = Path.Combine(SystemInfo.GetDefault().SystemFileLocation, "wine-download.tar.xz");
        if (!File.Exists(downloadPath))
        {
            var httpClient = new HttpClient();
            await httpClient.DownloadFileAsync(WineDownloadUrl, downloadPath);
        }
        
        // Extract the WINE .tar.xz using the tar command.
        // This is done with the system tar command to preserve symbolic links.
        var temporaryPath = Path.Combine(SystemInfo.GetDefault().SystemFileLocation, "wine-extract");
        if (Directory.Exists(temporaryPath))
        {
            Directory.Delete(temporaryPath, true);
        }
        Directory.CreateDirectory(temporaryPath);
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            FileName = "tar",
            Arguments = "xJf \"" + downloadPath.Replace("\"","\\\"") + "\" -C \"" + temporaryPath.Replace("\"","\\\"") + "\"",
        };
        process.Start();
        await process.WaitForExitAsync();
        
        // Move the WINE directory.
        if (Directory.Exists(WineExtractPath))
        {
            Directory.Delete(WineExtractPath, true);
        }
        Directory.Move(Path.Combine(temporaryPath, "Wine Stable.app", "Contents", "Resources", "wine"), WineExtractPath);
        
        // Clear the files.
        File.Delete(downloadPath);
        Directory.Delete(temporaryPath, true);
    }

    /// <summary>
    /// Runs an application in the emulator.
    /// </summary>
    /// <param name="executablePath">Path of the executable to run.</param>
    /// <param name="workingDirectory">Working directory to run the executable in.</param>
    /// <returns>The process of the runtime.</returns>
    public Process RunApplication(string executablePath, string workingDirectory)
    {
        // Create and return the process.
        Logger.Info("Starting with macOS download for WINE.");
        var clientProcess = new Process
        {
            StartInfo =
            {
                FileName = WinePath,
                Arguments = executablePath,
                WorkingDirectory = workingDirectory,
                CreateNoWindow = true,
            }
        };
        clientProcess.StartInfo.EnvironmentVariables["WINEDLLOVERRIDES"] = "dinput8.dll=n,b";
        return clientProcess;
    }
}