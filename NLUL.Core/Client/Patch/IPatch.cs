/*
 * TheNexusAvenger
 *
 * Interface for a client patch.
 */

namespace NLUL.Core.Client.Patch
{
    public interface IPatch
    {
        /*
         * Returns if an update is available.
         */
        public bool IsUpdateAvailable();
        
        /*
         * Returns if the patch is installed.
         */
        public bool IsInstalled();
        
        /*
         * Installs the patch.
         */
        public void Install();
        
        /*
         * Uninstalls the patch.
         */
        public void Uninstall();
    }
}