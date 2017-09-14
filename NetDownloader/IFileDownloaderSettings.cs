using System;

namespace NetDownloader
{
    public interface IFileDownloaderSettings
    {
        /// <summary>
        /// Chunk size
        /// </summary>
        int ChunkSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum waiting timeout for pending request to be finished. Default is 15 seconds.
        /// </summary>
        TimeSpan SafeWaitTimeout { get; set; }

        /// <summary>
        /// File storage
        /// </summary>
        IFileStorage FileStorage { get; set; }

        /// <summary>
        /// Destination Path
        /// </summary>
        string DestinationPath { get; set; }
    }
}