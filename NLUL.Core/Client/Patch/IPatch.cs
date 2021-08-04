namespace NLUL.Core.Client.Patch
{
    public interface IPatch
    {
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