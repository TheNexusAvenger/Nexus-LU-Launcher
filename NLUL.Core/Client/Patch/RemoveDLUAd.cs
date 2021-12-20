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
        /// Whether the patch is hidden in the list of patches.
        /// </summary>
        public bool Hidden => true; // Not intended to be undone. This is due to some archives adding this.

        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum => ClientPatchName.RemoveDLUAd;
        
        /// <summary>
        /// System info of the client.
        /// </summary>
        private readonly SystemInfo systemInfo;
        
        /// <summary>
        /// Location of the locale file.
        /// </summary>
        private string LocaleFileLocation => Path.Combine(systemInfo.ClientLocation, "locale", "locale.xml");
     
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable => false;

        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed => File.Exists(this.LocaleFileLocation) && !File.ReadAllText(this.LocaleFileLocation).Contains("DLU is coming!");
     
        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        public RemoveDLUAd(SystemInfo systemInfo)
        {
            this.systemInfo = systemInfo;
        }
        
        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install()
        {
            if (!File.Exists(this.LocaleFileLocation)) return;
            File.WriteAllText(this.LocaleFileLocation,
                File.ReadAllText(this.LocaleFileLocation)
                    .Replace("DLU is coming!", "Build on Nimbus Isle!")
                    .Replace("Follow us on Twitter", "Get inspired and build on Nimbus Station&apos;s largest Property!")
                    .Replace("@darkflameuniv", "Look for the launch pad by the water&apos;s edge in Brick Annexe!"));
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall()
        {
            if (!File.Exists(this.LocaleFileLocation)) return;
            File.WriteAllText(this.LocaleFileLocation,
                File.ReadAllText(this.LocaleFileLocation)
                    .Replace("Build on Nimbus Isle!", "DLU is coming!")
                    .Replace("Get inspired and build on Nimbus Station&apos;s largest Property!", "Follow us on Twitter")
                    .Replace("Look for the launch pad by the water&apos;s edge in Brick Annexe!", "@darkflameuniv"));
        }
    }
}