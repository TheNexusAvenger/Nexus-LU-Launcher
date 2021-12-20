namespace NLUL.Core.Client.Patch
{
    public interface IPatch
    {
        /// <summary>
        /// Name of the patch.
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Description of the patch.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Whether the patch is hidden in the list of patches.
        /// </summary>
        public bool Hidden { get; }
        
        /// <summary>
        /// Enum of the patch.
        /// </summary>
        public ClientPatchName PatchEnum { get; }
        
        /// <summary>
        /// Whether an update is available.
        /// </summary>
        public bool UpdateAvailable { get; }
        
        /// <summary>
        /// Whether the patch is installed
        /// </summary>
        public bool Installed { get; }
        
        /// <summary>
        /// Installs the patch.
        /// </summary>
        public void Install();
        
        /// <summary>
        /// Uninstalls the patch.
        /// </summary>
        public void Uninstall();
    }
}