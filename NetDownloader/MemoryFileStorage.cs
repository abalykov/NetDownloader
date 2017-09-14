using System;
using System.Collections.Concurrent;

namespace NetDownloader
{
    public class MemoryFileStorage : IFileStorage
    {
        private ConcurrentDictionary<Uri, FileStorageItem> cache = new ConcurrentDictionary<Uri, FileStorageItem>();
        public void Add(Uri uri, FileStorageItem streamItem)
        {
            this.cache.TryAdd(uri, streamItem);
        }

        public FileStorageItem Get(Uri uri)
        {
            FileStorageItem ret = null;
            this.cache.TryGetValue(uri, out ret);
            
            return ret;
        }

        public void Invalidate(Uri uri)
        {
            FileStorageItem item;
            this.cache.TryRemove(uri, out item);
        }
    }
}