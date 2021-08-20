using System.IO;

namespace NLUL.Core.Client.Patch
{
    public class FixAvantGardensSurvivalCrash : IPatch
    {
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name => "Fix Avant Gardens Survival Crash";
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        public string Description => "Fixes a mistake in the Avant Gardens Survival script that results in players crashing in Avant Gardens Survival if they are not the first player.";
        
        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum => ClientPatchName.FixAvantGardensSurvivalCrash;
        
        /// <summary>
        /// Location of the Avant Gardens Survival client file.
        /// </summary>
        private readonly string survivalScriptFileLocation;
     
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable => false;

        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed => File.Exists(this.survivalScriptFileLocation) && !File.ReadAllText(this.survivalScriptFileLocation).Contains("    PlayerReady(self)");
     
        /// <summary>
        /// Creates the patch.
        /// </summary>
        /// <param name="systemInfo">System info of the client.</param>
        public FixAvantGardensSurvivalCrash(SystemInfo systemInfo)
        {
            this.survivalScriptFileLocation = Path.Combine(systemInfo.ClientLocation, "res", "scripts", "ai", "minigame", "survival", "l_zone_survival_client.lua");
        }
        
        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install()
        {
            if (!File.Exists(this.survivalScriptFileLocation)) return;
            File.WriteAllText(this.survivalScriptFileLocation,
                File.ReadAllText(this.survivalScriptFileLocation)
                    .Replace("    PlayerReady(self)", "    onPlayerReady(self)"));
        }
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall()
        {
            if (!File.Exists(this.survivalScriptFileLocation)) return;
            File.WriteAllText(this.survivalScriptFileLocation,
                File.ReadAllText(this.survivalScriptFileLocation)
                    .Replace("    onPlayerReady(self)", "    PlayerReady(self)"));
        }
    }
}