using System;

namespace NLUL.Core.Client.Archive
{
    public abstract class ClientArchive
    {
        /// <summary>
        /// Delegate for an event handler with no parameters.
        /// </summary>
        /// <param name="progress">Progress of the extracting.</param>
        public delegate void ExtractProgressEventHandler(float progress);
        
        /// <summary>
        /// Event for the server list changing.
        /// </summary>
        public event ExtractProgressEventHandler ExtractProgress;

        /// <summary>
        /// Location of the archive file.
        /// </summary>
        public readonly string ArchiveFile;

        /// <summary>
        /// Required difference since the last ExtractProgress call to raise the event.
        /// </summary>
        public float ReportedProgressBuffer { get; set; }= 0.01f;

        /// <summary>
        /// Last progress that was reported.
        /// </summary>
        private float lastReportedProgress = 0;

        /// <summary>
        /// Creates the client archive.
        /// </summary>
        /// <param name="archiveFile">Location of the archive file.</param>
        public ClientArchive(string archiveFile)
        {
            this.ArchiveFile = archiveFile;
        }

        /// <summary>
        /// Reports progress of extracting.
        /// </summary>
        /// <param name="progress">Progress of the extracting.</param>
        internal void ReportExtractingProgress(float progress)
        {
            // Return if the last reported progress is close.
            // Due to this event invoking UI updates, this can't be passed through every call.
            if (progress != 0 && progress < 1 && Math.Abs(progress - this.lastReportedProgress) < this.ReportedProgressBuffer) return;
            
            // Invoke the event.
            this.lastReportedProgress = progress;
            this.ExtractProgress?.Invoke(progress);
        }
        
        /// <summary>
        /// Determines if an archive contains a client.
        /// </summary>
        /// <returns>Whether the archive contains a client.</returns>
        public abstract bool ContainsClient();

        /// <summary>
        /// Extracts the client in an archive to a directory.
        /// </summary>
        /// <param name="targetLocation">Location to extract to</param>
        public abstract void ExtractTo(string targetLocation);
        
        /// <summary>
        /// Verifies the client in a directory is extracted correctly.
        /// </summary>
        /// <param name="targetLocation">Location to verify.</param>
        /// <returns>Whether the extract was verified.</returns>
        public abstract bool Verify(string targetLocation);
    }
}