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
        /// Source of the client.
        /// </summary>
        public ClientSourceEntry Source { get; }
        
        /// <summary>
        /// Whether the extracted client can be verified.
        /// </summary>
        public abstract bool CanVerifyExtractedClient { get; }

        /// <summary>
        /// Size of the client to download. Intended to be set after
        /// a download starts.
        /// </summary>
        public abstract long ClientDownloadSize { get; protected set; }

        /// <summary>
        /// Size of the client that has been downloaded.
        /// </summary>
        public abstract long DownloadedClientSize { get; }

        /// <summary>
        /// Creates the download method.
        /// </summary>
        /// <param name="systemInfo">Information about the system.</param>
        /// <param name="source">Source of the client.</param>
        public DownloadMethod(SystemInfo systemInfo, ClientSourceEntry source)
        {
            this.SystemInfo = systemInfo;
            this.Source = source;
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
        public abstract void Download();

        /// <summary>
        /// Verifies the extracted client.
        /// </summary>
        /// <returns>Whether the client was verified.</returns>
        public abstract bool Verify();
    }
}