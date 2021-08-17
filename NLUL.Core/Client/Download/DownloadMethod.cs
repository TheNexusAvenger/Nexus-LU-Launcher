using System;
using NLUL.Core.Client.Source;

namespace NLUL.Core.Client.Download
{
    public abstract class DownloadMethod
    {
        /// <summary>
        /// Event for the state changing.
        /// </summary>
        public event EventHandler<string> DownloadStateChanged;
        
        /// <summary>
        /// Information about the system.
        /// </summary>
        public SystemInfo SystemInfo { get; }

        /// <summary>
        /// Creates the download method.
        /// </summary>
        /// <param name="systemInfo">Information about the system.</param>
        public DownloadMethod(SystemInfo systemInfo)
        {
            this.SystemInfo = systemInfo;
        }
        
        /// <summary>
        /// Invokes the download state changing.
        /// </summary>
        /// <param name="state">New state that was reached.</param>
        protected void OnDownloadStateChanged(string state)
        {
            this.DownloadStateChanged?.Invoke(this, state);
        }
        
        /// <summary>
        /// Downloads and extracts the client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        public abstract void Download(ClientSourceEntry source);
    }
}