using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetDownloader.Console.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://download.microsoft.com/download/6/4/8/648EB83C-00F9-49B2-806D-E46033DA4AE6/ExchangeServer2016-CU1.iso");
            var cts = new CancellationTokenSource();

            var token = cts.Token;
            var downloader = new FileDownloader(token,
                new FileDownloaderSettings {
                    ChunkSize = 1024,
                    DestinationPath = @"c:\temp\",
                    SafeWaitTimeout = new TimeSpan(0, 0, 5),
                    FileStorage = new MemoryFileStorage()
                });

            downloader.DownloadFileCompleted += Downloader_DownloadFileCompleted;
            downloader.DownloadProgressChanged += Downloader_DownloadProgressChanged;

            downloader.DownloadFileAsync(uri, true);


        }

        private static void Downloader_DownloadProgressChanged(object sender, DownloadFileProgressChangedArgs e)
        {
            //throw new NotImplementedException();
        }

        private static void Downloader_DownloadFileCompleted(object sender, DownloadFileCompletedArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
