/*
 * TheNexusAvenger
 *
 * Patch for removing "DLU is coming!" ad from the zone loading tips.
 */

using System.IO;
using System.Net;
using NLUL.Core.Util;

namespace NLUL.Core.Client.Patch
{
    public class RemoveDLUAd : IPatch
    {
        private string localeFileLocation;
     
        /*
         * Creates the patch.
         */
        public RemoveDLUAd(SystemInfo systemInfo)
        {
            this.localeFileLocation = Path.Combine(systemInfo.ClientLocation, "locale", "locale.xml");
        }
        
        /*
         * Returns if an update is available.
         */
        public bool IsUpdateAvailable()
        {
            return false;
        }
        
        /*
         * Returns if the patch is installed.
         */
        public bool IsInstalled()
        {
            return !File.Exists(this.localeFileLocation) || !File.ReadAllText(this.localeFileLocation).Contains("DLU is coming!");
        }
        
        /*
         * Installs the patch.
         */
        public void Install()
        {
            if (!File.Exists(this.localeFileLocation)) return;
            File.WriteAllText(this.localeFileLocation,
                File.ReadAllText(this.localeFileLocation)
                    .Replace("DLU is coming!", "Build on Nimbus Isle!")
                    .Replace("Follow us on Twitter", "Get inspired and build on Nimbus Station&apos;s largest Property!")
                    .Replace("@darkflameuniv", "Look for the launch pad by the water&apos;s edge in Brick Annexe!"));
        }
        
        /*
         * Uninstalls the patch.
         */
        public void Uninstall()
        {
            if (!File.Exists(this.localeFileLocation)) return;
            File.WriteAllText(this.localeFileLocation,
                File.ReadAllText(this.localeFileLocation)
                    .Replace("Build on Nimbus Isle!", "DLU is coming!")
                    .Replace("Get inspired and build on Nimbus Station&apos;s largest Property!", "Follow us on Twitter")
                    .Replace("Look for the launch pad by the water&apos;s edge in Brick Annexe!", "@darkflameuniv"));
        }
    }
}