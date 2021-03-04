/*
 * TheNexusAvenger
 *
 * Interface for a patch that applies before
 * launching.
 */

namespace NLUL.Core.Client.Patch
{
    public interface IPreLaunchPatch : IPatch
    {
        /*
         * Performs and operations between setting the
         * boot.cfg and launching the client. This will
         * yield launching the client.
         */
        public void OnClientRequestLaunch();
    }
}