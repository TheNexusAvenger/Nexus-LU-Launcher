using System;
using System.IO;
using System.Net;
using NLUL.Core.Client.Source;

namespace NLUL.Core.Client.Download
{
    public abstract class FileDownloadMethod : DownloadMethod
    {
        /// <summary>
        /// Creates the download method.
        /// </summary>
        /// <param name="systemInfo">Information about the system.</param>
        public FileDownloadMethod(SystemInfo systemInfo) : base(systemInfo)
        {
            
        }

        /// <summary>
        /// Extract location of the client.
        /// </summary>
        public string ExtractLocation => Path.Combine(this.SystemInfo.SystemFileLocation, "ClientExtract");
        
        /// <summary>
        /// Returns the download location.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        /// <returns>Returns the download location.</returns>
        public string GetDownloadLocation(ClientSourceEntry source)
        {
            return Path.Combine(this.SystemInfo.SystemFileLocation, "client." + source.Method.ToLower());
        }
        
        /// <summary>
        /// Downloads the client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        /// <param name="force">Whether to force download.</param>
        private void DownloadClient(ClientSourceEntry source, bool force)
        {
            // Return if the client exists and isn't forced.
            var downloadLocation = this.GetDownloadLocation(source);
            if (!force && File.Exists(downloadLocation))
            {
                return;
            }
            
            // Delete the existing download if it exists.
            if (File.Exists(downloadLocation))
            {
                File.Delete(downloadLocation);
            }
            
            // Download the client.
            Directory.CreateDirectory(Path.GetDirectoryName(this.SystemInfo.ClientLocation));
            var client = new WebClient();
            client.DownloadFile(source.Url, downloadLocation);
        }
        
        /// <summary>
        /// Downloads and extracts the client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        public override void Download(ClientSourceEntry source)
        {
            // Download the client if not done already.
            this.OnDownloadStateChanged("Download");
            this.DownloadClient(source, false);
            
            try
            {
                // Try to extract the existing file.
                this.OnDownloadStateChanged("Extract");
                this.Extract(source);
            }
            catch (InvalidOperationException)
            {
                // Re-download the client.
                this.OnDownloadStateChanged("Download");
                this.DownloadClient(source, true);
                this.OnDownloadStateChanged("Extract");
                this.Extract(source);
            }
            
            // Verify the client.
            this.OnDownloadStateChanged("Verify");
            this.Verify(source);
            File.Delete(this.GetDownloadLocation(source));
        }

        /// <summary>
        /// Extracts the downloaded client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        public abstract void Extract(ClientSourceEntry source);

        /// <summary>
        /// Verifies the extracted client.
        /// </summary>
        /// <param name="source">Source of the client.</param>
        public abstract void Verify(ClientSourceEntry source);
    }
}