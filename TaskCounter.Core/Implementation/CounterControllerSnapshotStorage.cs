using System;
using System.IO;
using System.IO.Compression;

using GroBuf;

using log4net;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.LocalPersistentStoring;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class CounterControllerSnapshotStorage : ICounterControllerSnapshotStorage
    {
        public CounterControllerSnapshotStorage(Func<string, long, ILocalPersistentStorage<SnapshotData>> createPersistentFileStorage,
                                                IApplicationSettings applicationSettings,
                                                ISerializer serializer)
        {
            this.serializer = serializer;
            string path;
            if(!applicationSettings.TryGetString("SnapshotStoragePath", out path))
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Storage");
            logger.InfoFormat("Path: {0}", path);
            fileStorage = createPersistentFileStorage(path, maxSize);
        }

        private static byte[] Decompress(byte[] rawData)
        {
            var uncompressed = new MemoryStream();
            using(var gZipStream = new GZipStream(new MemoryStream(rawData, false), CompressionMode.Decompress))
                gZipStream.CopyTo(uncompressed);
            return uncompressed.ToArray();
        }

        private static byte[] Compress(byte[] bytes)
        {
            var compressedSource = new MemoryStream();
            using(var gZipStream = new GZipStream(compressedSource, CompressionMode.Compress))
                gZipStream.Write(bytes, 0, bytes.Length);
            return compressedSource.ToArray();
        }

        public void SaveSnapshot(CounterControllerSnapshot snapshot)
        {
            logger.LogInfoFormat("Saving snapshot");
            fileStorage.Write(new SnapshotData()
                {
                    Version = currentVersion,
                    Data = Compress(serializer.Serialize(snapshot))
                });
        }

        public CounterControllerSnapshot ReadSnapshotOrNull()
        {
            SnapshotData result;
            if(!fileStorage.TryRead(out result))
            {
                logger.LogWarnFormat("No snapshot found");
                return null;
            }
            if(result.Version == currentVersion)
            {
                logger.LogWarnFormat("Snapshot ok. version={0}", result.Version);
                var decompress = Decompress(result.Data);
                return serializer.Deserialize<CounterControllerSnapshot>(decompress);
            }
            logger.LogWarnFormat("Snapshot has old version. version={0}", result.Version);
            return null;
        }

        private const int currentVersion = 1;
        private const int maxSize = 10 * 1024 * 1024;
        private static readonly ILog logger = LogManager.GetLogger("SnapshotStorage");
        private readonly ISerializer serializer;
        private readonly ILocalPersistentStorage<SnapshotData> fileStorage;

        public class SnapshotData
        {
            public int Version { get; set; }
            public byte[] Data { get; set; }
        }
    }
}