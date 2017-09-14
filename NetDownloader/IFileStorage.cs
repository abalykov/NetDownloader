using System;

namespace NetDownloader
{
    public interface IFileStorage
    {
        /// <summary>
        /// Invalidate cache for specific url
        /// </summary>
        /// <param name="uri">URI to invalidate</param>
        void Invalidate(Uri uri);

        /// <summary>
        /// Add new cache record
        /// </summary>
        /// <param name="uri">Source URI</param>
        void Add(Uri uri, FileStorageItem streamItem);

        /// <summary>
        /// Get the file from cache. Return file name if file is found in cache, null otherwise 
        /// </summary>
        /// <param name="uri">Source uri</param>
        /// <returns>Path to file with cached resource</returns>
        FileStorageItem Get(Uri uri);
    }
}