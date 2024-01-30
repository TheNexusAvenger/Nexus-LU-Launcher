using System.Diagnostics;
using System.Threading.Tasks;
using Nexus.LU.Launcher.State.Enum;

namespace Nexus.LU.Launcher.State.Client.Runtime;

public interface IRuntime
{
    /// <summary>
    /// Name of the runtime.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// State of the runtime.
    /// </summary>
    public RuntimeState RuntimeState { get; }
        
    /// <summary>
    /// Attempts to install the emulator.
    /// </summary>
    public Task InstallAsync();
        
    /// <summary>
    /// Runs an application in the emulator.
    /// </summary>
    /// <param name="executablePath">Path of the executable to run.</param>
    /// <param name="workingDirectory">Working directory to run the executable in.</param>
    /// <returns>The process of the runtime.</returns>
    public Process RunApplication(string executablePath, string workingDirectory);
}