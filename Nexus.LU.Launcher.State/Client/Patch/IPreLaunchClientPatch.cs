using System.Threading.Tasks;

namespace Nexus.LU.Launcher.State.Client.Patch;

public interface IPreLaunchClientPatch : IClientPatch
{
    /// <summary>
    /// Performs operations between setting the boot.cfg and launching
    /// the client. This will yield launching the client.
    /// </summary>
    public Task OnClientRequestLaunchAsync();
}