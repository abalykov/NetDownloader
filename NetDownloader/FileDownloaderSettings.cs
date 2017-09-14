using System;

namespace NetDownloader
{
    public class FileDownloaderSettings : IFileDownloaderSettings
    {
        public FileDownloaderSettings()
        {
        }

        public int ChunkSize { get; set; }
        public TimeSpan SafeWaitTimeout { get; set; }
        public IFileStorage FileStorage { get; set; }
        public string DestinationPath { get; set; }
    }
}