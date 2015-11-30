using System;

using GroBuf;

using log4net;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Repositories.BlobStorages
{
    public class TaskDataBlobStorage : IntermediateBlobStorageDecorator<byte[]>, ITaskDataBlobStorage
    {
        public TaskDataBlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName, timeBasedColumnFamilyName)
        {
            keyspace = parameters.Settings.QueueKeyspace;
        }

        public string GenerateBlobId(byte[] blob)
        {
            if(blob.Length <= TimeBasedBlobStorageSettings.BlobSizeLimit)
                return TimeGuid.NowGuid().ToGuid().ToString();

            var taskId = Guid.NewGuid().ToString();
            logger.WarnFormat("Blob with id={0} has size={1} bytes. Cannot write to timeBasedColumnFamily in keyspace:{2}.", taskId, blob.Length, keyspace);
            return taskId;
        }

        public const string columnFamilyName = "taskDataStorage";
        public const string timeBasedColumnFamilyName = "timeBasedTaskDataStorage";
        private readonly string keyspace;
        private static readonly ILog logger = LogManager.GetLogger(typeof(RemoteTaskQueue));
    }
}