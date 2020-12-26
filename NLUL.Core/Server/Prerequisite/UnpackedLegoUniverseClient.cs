/*
 * TheNexusAvenger
 *
 * Prerequisite that requires an unpacked Lego Universe client.
 */

using System.IO;
using System.Linq;
using NLUL.Core.Client;

namespace NLUL.Core.Server.Prerequisite
{
    public class UnpackedLegoUniverseClient : IPrerequisite
    {
        private SystemInfo systemInfo;
        
        /*
         * Creates a Unpacked Client Prerequisite object.
         */
        public UnpackedLegoUniverseClient(SystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
        }
        
        /*
         * Returns the name of the prerequisite.
         */
        public string GetName()
        {
             return "Unpacked Lego Universe Client";
        }
        
        /*
         * Returns the error message for the
         * prerequisite not being met.
         */
        public string GetErrorMessage()
        {
            return "The Lego Universe client was not downloaded.";
        }
        
        /*
         * Handles setting up the prerequisite.
         * Returns if it was completed successfully,
         * and false if it wasn't.
         */
        public bool SetupPrerequisite()
        {
            // Download and patch the client.
            var client = new ClientRunner(this.systemInfo);
            client.TryExtractClient(false);
            client.PatchClient();
            
            // Return true.
            return true;
        }
        
        /*
         * Returns if the prerequisite was met.
         */
        public bool IsMet()
        {
            // Download the unpacked client if it isn't detected.
            if (!Directory.Exists(this.systemInfo.ClientLocation) || !Directory.GetFiles(this.systemInfo.ClientLocation,"*.luz",SearchOption.AllDirectories).Any())
            {
                this.SetupPrerequisite();
            }
            
            // Return true (should install).
            return true;
        }
    }
}