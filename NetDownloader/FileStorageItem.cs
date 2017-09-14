using System.IO;
using System.Net;

namespace NetDownloader
{
    public class FileStorageItem
    {
        public Stream stream { get; set; }
        public WebHeaderCollection headers { get; set; }
    }
}