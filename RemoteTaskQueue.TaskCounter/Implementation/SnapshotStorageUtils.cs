using System.IO;
using System.IO.Compression;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class SnapshotStorageUtils
    {
        public static byte[] Decompress(byte[] rawData)
        {
            var uncompressed = new MemoryStream();
            using(var gZipStream = new GZipStream(new MemoryStream(rawData, false), CompressionMode.Decompress))
                gZipStream.CopyTo(uncompressed);
            return uncompressed.ToArray();
        }

        public static byte[] Compress(byte[] bytes)
        {
            var compressedSource = new MemoryStream();
            using(var gZipStream = new GZipStream(compressedSource, CompressionMode.Compress))
                gZipStream.Write(bytes, 0, bytes.Length);
            return compressedSource.ToArray();
        }
    }
}