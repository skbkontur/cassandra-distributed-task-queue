using System.IO;
using System.IO.Compression;

using GroBuf;

using log4net;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class SnapshotConverter
    {
        public SnapshotConverter(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        public byte[] ConvertToBytes(InternalSnapshot internalSnapshot)
        {
            var bytes = serializer.Serialize(internalSnapshot);
            var compressedSource = new MemoryStream();
            using(var gZipStream = new GZipStream(compressedSource, CompressionMode.Compress))
                gZipStream.Write(bytes, 0, bytes.Length);
            return compressedSource.ToArray();
        }

        public InternalSnapshot ConvertFromBytes(int version, byte[] rawData)
        {
            logger.InfoFormat("Snapshot version: {0}", version);
            switch(version)
            {
            case 0: //note 0 == v1 beacase v1 has no verion comumn
            case 1:
                return ConvertFromV1(rawData);
            case CurrentVersion:
                return ConvertFromV2(rawData);
            default:
                logger.WarnFormat("Unsupported snapshot version: {0}", version);
                return null;
            }
        }

        public readonly InternalSnapshot emptyOldSnapshot = new InternalSnapshot(new MetaProvider.MetaProviderSnapshot(0, 0, null, null), new ProcessedTasksCounter.CounterSnapshot(null, 0, 0));

        public const int CurrentVersion = 2;

        private InternalSnapshot ConvertFromV2(byte[] rawData)
        {
            var uncompressed = new MemoryStream();
            using(var gZipStream = new GZipStream(new MemoryStream(rawData, false), CompressionMode.Decompress))
                gZipStream.CopyTo(uncompressed);
            return serializer.Deserialize<InternalSnapshot>(uncompressed.ToArray());
        }

        private InternalSnapshot ConvertFromV1(byte[] rawData)
        {
            return serializer.Deserialize<InternalSnapshot>(rawData);
        }

        private static readonly ILog logger = LogManager.GetLogger("SnapshotConverter");

        private readonly ISerializer serializer;
    }
}