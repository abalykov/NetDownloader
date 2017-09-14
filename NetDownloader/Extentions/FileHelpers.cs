using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetDownloader
{
    internal static class FileHelpers
    {
        public static bool TryGetFileSize(string filename, out long filesize)
        {
            try
            {
                var fileInfo = new FileInfo(filename);
                filesize = fileInfo.Length;
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to get file size for {0}. Exception: {1}", filename, e.Message);
                filesize = 0;
                return false;
            }
            return true;
        }

        public static bool TryFileDelete(string filename)
        {
            try
            {
                File.Delete(filename);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Unable to delete file {0}. Exception: {1}", filename, e.Message);
                return false;
            }
            return true;
        }

        public static bool ReplaceFile(string source, string destination)
        {
            if (!destination.Equals(source, StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    File.Delete(destination);
                    File.Move(source, destination);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Unable replace local file {0} with cached resource {1}, {2}", destination, source, e.Message);
                    return false;
                }
            }
            return true;
        }
    }
}
