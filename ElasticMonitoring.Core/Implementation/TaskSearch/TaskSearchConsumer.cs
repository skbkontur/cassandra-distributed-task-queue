using System;
using System.Linq;
using System.Threading;

using GroBuf;

using log4net;

using MoreLinq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.MetaProviding;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.TaskSearch
{
    public class TaskSearchConsumer : IMetaConsumer, IDisposable
    {
        public TaskSearchConsumer(ITaskDataBlobStorage taskDataStorage,
                                  ITaskDataTypeToNameMapper taskDataTypeToNameMapper,
                                  ISerializer serializer,
                                  TaskSearchIndex searchIndex,
                                  IRemoteLockCreator remoteLockCreator,
                                  LastReadTicksStorage lastReadTicksStorage,
                                  IMetaLoaderFactory metaLoaderFactory,
                                  ICurrentMetaProvider currentMetaProvider)
        {
            this.taskDataStorage = taskDataStorage;
            this.taskDataTypeToNameMapper = taskDataTypeToNameMapper;
            this.serializer = serializer;
            this.searchIndex = searchIndex;
            this.remoteLockCreator = remoteLockCreator;
            this.lastReadTicksStorage = lastReadTicksStorage;
            metaLoadController = new MetaLoadController(currentMetaProvider, this, metaLoaderFactory, TaskSearchSettings.SyncLoadInterval.Ticks, "TaskSearchConsumer");
        }

        public void Dispose()
        {
            if(distributedLock != null)
            {
                //NOTE close lock if shutdown by container
                distributedLock.Dispose();
                distributedLock = null;
                logger.InfoFormat("Distributed lock released");
            }
        }

        public void ProcessMetas(TaskMetaInformation[] metas, long readTicks)
        {
            IndexMetas(metas);
            var estimatedLastUpdateReadTicks = GetEstimatedLastUpdateReadTicks(metas);
            if(estimatedLastUpdateReadTicks.HasValue)
                lastReadTicksStorage.SetLastReadTicks(estimatedLastUpdateReadTicks.Value);
            else
                lastReadTicksStorage.SetLastReadTicks(readTicks);
        }

        public void ProcessQueue()
        {
            if(DistributedLockAcquired())
                metaLoadController.ProcessQueue();
        }

        private static long? GetEstimatedLastUpdateReadTicks(TaskMetaInformation[] metas)
        {
            var minTicks = long.MaxValue;
            foreach(var meta in metas)
                minTicks = Math.Min(minTicks, meta.LastModificationTicks.Value);
            if(minTicks == long.MaxValue)
                return null;
            return minTicks;
        }

        private bool DistributedLockAcquired()
        {
            if(distributedLock != null)
                return true;
            IRemoteLock @lock;
            if(remoteLockCreator.TryGetLock(lockId, out @lock))
            {
                var lastReadTicks = lastReadTicksStorage.GetLastReadTicks();
                metaLoadController.ResetTo(lastReadTicks);
                Interlocked.Exchange(ref startTicks, lastReadTicks);
                distributedLock = @lock;
                logger.InfoFormat("Distributed lock acquired. LastUpdateTicks={0}", DateTimeFormatter.FormatWithMsAndTicks(lastReadTicks));
                return true;
            }
            return false;
        }

        private void IndexMetas(TaskMetaInformation[] metas)
        {
            metas.Batch(TaskSearchSettings.BulkBatchSize).ForEach(enumerable => IndexMetaBatch(enumerable.ToArray()));
        }

        private void IndexMetaBatch(TaskMetaInformation[] batch)
        {
            var taskDatas = taskDataStorage.ReadQuiet(batch.Select(m => m.Id).ToArray());
            var taskDataObjects = new object[taskDatas.Length];
            for(var i = 0; i < batch.Length; i++)
            {
                var taskData = taskDatas[i];
                Type taskType;
                object taskDataObj = null;
                if(taskDataTypeToNameMapper.TryGetTaskType(batch[i].Name, out taskType))
                    taskDataObj = serializer.Deserialize(taskType, taskData);
                taskDataObjects[i] = taskDataObj;
            }
            if(batch.Length > 0)
                searchIndex.IndexBatch(batch, taskDataObjects);
        }

        private const string lockId = "TaskSearch_Loading_Lock";

        private static readonly ILog logger = LogManager.GetLogger("TaskSearchConsumer");

        private long startTicks;

        private volatile IRemoteLock distributedLock;

        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly ITaskDataTypeToNameMapper taskDataTypeToNameMapper;
        private readonly ISerializer serializer;
        private readonly TaskSearchIndex searchIndex;

        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly LastReadTicksStorage lastReadTicksStorage;

        private readonly MetaLoadController metaLoadController;
    }
}