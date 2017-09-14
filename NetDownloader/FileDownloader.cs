using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetDownloader
{
    public class FileDownloader
    {
        private readonly ManualResetEvent readyToDownload = new ManualResetEvent(true);
        private bool useFileNameFromServer;
        private Uri fileSource;
        private string destinationFileName;
        private DownloadWebClient downloadWebClient;

        public FileDownloader(CancellationToken cancel, IFileDownloaderSettings settings)
        {
            Cancel = cancel;
            Settings = settings;
        }

        public CancellationToken Cancel { get; }
        public IFileDownloaderSettings Settings { get; }
        
        /// <summary>
        /// Gets the total bytes received so far
        /// </summary>
        public long BytesReceived { get; internal set; }

        private bool isCancelled;
        private string localFileName;
        private bool isFallback;

        /// <summary>
        /// Gets the total bytes to receive
        /// </summary>
        public long TotalBytesToReceive { get; internal set; }


        /// <summary>
        /// Fired when download is finished, even if it's failed.
        /// </summary>
        public event EventHandler<DownloadFileCompletedArgs> DownloadFileCompleted;

        /// <summary>
        /// Fired when download progress is changed.
        /// </summary>
        public event EventHandler<DownloadFileProgressChangedArgs> DownloadProgressChanged;

        public void DownloadFileAsync(Uri source, bool useServerFileName)
        {
            if (!WaitSafeStart())
            {
                throw new Exception("Unable to start download because another request is still in progress.");
            }

            Debug.WriteLine("Download File '{0}' is called.", source);

            this.useFileNameFromServer = useServerFileName;
            this.fileSource = source;
            BytesReceived = 0;
            this.isCancelled = false;
            this.localFileName = Path.Combine(Settings.DestinationPath, string.Format("{0}.downloading", Guid.NewGuid())); ;

            StartDownload();
        }

        public void CancelDownloadAsync()
        {
            Debug.WriteLine("CancelDownloadAsync called.");

            TriggerDownloadWebClientCancelAsync();
            DeleteDownloadedFile();  ////todo: maybe this is equal to InvalidateCache? Can we get rid of DeleteDownloadedFile ?

            this.readyToDownload.Set();
        }

        private void TriggerDownloadWebClientCancelAsync()
        {
            if (this.downloadWebClient != null)
            {
                this.downloadWebClient.CancelAsync();
                this.downloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;

                Debug.WriteLine("Successfully cancelled web client.");
            }
        }


        private void StartDownload()
        {
            TotalBytesToReceive = -1;
            var headers = GetHttpHeaders(this.fileSource);
            if (headers != null)
            {
                TotalBytesToReceive = headers.GetContentLength();
            }

            if (TotalBytesToReceive == -1)
            {
                TotalBytesToReceive = 0;
                Debug.WriteLine("Received no Content-Length header from server for {0}. Cache is not used, Resume is not supported", this.fileSource);
                TriggerWebClientDownloadFileAsync();
            }
            else
            {
                ResumeDownload();
            }
        }

        private void TriggerWebClientDownloadFileAsync()
        {
            Debug.WriteLine("Falling back to legacy DownloadFileAsync.");
            try
            {
                this.isFallback = true;
                var destinationDirectory = Path.GetDirectoryName(this.localFileName);
                if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }
                TryCleanupExistingDownloadWebClient();

                this.downloadWebClient = CreateWebClient();
                this.downloadWebClient.DownloadFileAsync(this.fileSource, this.localFileName);
                Debug.WriteLine("Download async started. Source: {0} Destination: {1}", this.fileSource, this.localFileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to download Source:{0}, Destination:{1}, Error:{2}.", this.fileSource, this.localFileName, ex.Message);
            }
        }

        private void TryCleanupExistingDownloadWebClient()
        {
            if (this.downloadWebClient == null)
            {
                return;
            }
            try
            {
                lock (this)
                {
                    if (this.downloadWebClient != null)
                    {
                        this.downloadWebClient.DownloadFileCompleted -= OnDownloadCompleted;
                        this.downloadWebClient.DownloadProgressChanged -= OnDownloadProgressChanged;
                        this.downloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
                        this.downloadWebClient.CancelAsync();
                        this.downloadWebClient.Dispose();
                        this.downloadWebClient = null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error while cleaning up web client : {0}", e.Message);
            }
        }



        private DownloadWebClient CreateWebClient()
        {
            var webClient = new DownloadWebClient();
            webClient.DownloadFileCompleted += OnDownloadCompleted;
            webClient.DownloadProgressChanged += OnDownloadProgressChanged;
            webClient.OpenReadCompleted += OnOpenReadCompleted;
            return webClient;
        }

        private void OnOpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ResumeDownload()
        {
            
        }

        private FileStorageItem GetDestinationFile()
        {
            var cached = Settings.FileStorage.Get(this.fileSource);
            if (cached == null)
            {
                Debug.WriteLine("No cache item found. Source: {0} Destination: {1}", this.fileSource, this.localFileName);
                DeleteDownloadedFile();
                return null;
            }

            Debug.WriteLine("Download resource was found in cache. Source: {0} Destination: {1}", this.fileSource, cached);
            return cached;
        }

        private void DeleteDownloadedFile()
        {
            FileHelpers.TryFileDelete(this.localFileName);
        }


        private bool WaitSafeStart()
        {
            Debug.WriteLine("Calling DownloadFileAsync...");
            if (!this.readyToDownload.WaitOne(Settings.SafeWaitTimeout))
            {
                Debug.WriteLine("Failed to call DownloadFileAsync, another request is in progress: Source:{0}, Destination:{1}", this.fileSource, this.localFileName);
                return false;
            }
            this.readyToDownload.Reset();
            return true;
        }

        private WebHeaderCollection GetHttpHeaders(Uri source)
        {
            try
            {
                var webRequest = WebRequest.Create(source);
                webRequest.Method = WebRequestMethods.Http.Head;

                using (var webResponse = webRequest.GetResponse())
                {
                    return webResponse.Headers;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to read http headers for {0}: {1}; typeof(Exception)={2}", source, e.Message, e.GetType());
                return null;
            }
        }
    }
}
