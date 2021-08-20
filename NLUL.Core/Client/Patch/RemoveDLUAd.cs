using System.IO;

namespace NLUL.Core.Client.Patch
{
    public class RemoveDLUAd : IPatch
    {
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name => "Remove DLU Ad";
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        public string Description => "Removes the advertisement for DLU from the zone loading screen.";

        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum => ClientPatchName.RemoveDLUAd;
        
        /// <summary>
        /// Location of the locale file.
        /// </summary>
        private readonly string localeFileLocation;
     
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable => false;

        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed => File.Exists(this.localeFileLocation) && !File.ReadAllText(this.localeFileLocation).Contains("DLU is coming!");
     
        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        public RemoveDLUAd(SystemInfo systemInfo)
        {
            this.localeFileLocation = Path.Combine(systemInfo.ClientLocation, "locale", "locale.xml");
        }
        
        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install()
        {
            if (!File.Exists(this.localeFileLocation)) return;
            File.WriteAllText(this.localeFileLocation,
                File.ReadAllText(this.localeFileLocation)
                    .Replace("DLU is coming!", "Build on Nimbus Isle!")
                    .Replace("Follow us on Twitter", "Get inspired and build on Nimbus Station&apos;s largest Property!")
                    .Replace("@darkflameuniv", "Look for the launch pad by the water&apos;s edge in Brick Annexe!"));
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
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