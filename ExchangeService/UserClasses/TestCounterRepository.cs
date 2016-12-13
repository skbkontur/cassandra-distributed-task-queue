using System;

using GroBuf;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Settings;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace ExchangeService.UserClasses
{
    public class TestCounterRepository : ITestCounterRepository
    {
        public TestCounterRepository(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings taskQueueSettings, IGlobalTime globalTime, IRemoteLockCreator remoteLockCreator)
        {
            this.globalTime = globalTime;
            this.remoteLockCreator = remoteLockCreator;
            storage = new LegacyBlobStorage<int>(cassandraCluster, serializer, taskQueueSettings.QueueKeyspace, ColumnFamilies.TestCounterRepositoryCfName);
        }

        public int GetCounter(string taskId)
        {
            using(Lock(taskId))
                return GetCounterInternal(taskId);
        }

        public int IncrementCounter(string taskId)
        {
            using(Lock(taskId))
            {
                var cnt = GetCounterInternal(taskId);
                SetCounterInternal(taskId, cnt + 1);
                return cnt + 1;
            }
        }

        public int DecrementCounter(string taskId)
        {
            using(Lock(taskId))
            {
                var cnt = GetCounterInternal(taskId);
                SetCounterInternal(taskId, cnt - 1);
                return cnt - 1;
            }
        }

        public void SetValueForCounter(string taskId, int value)
        {
            using(Lock(taskId))
                SetCounterInternal(taskId, value);
        }

        private int GetCounterInternal(string taskId)
        {
            return storage.Read(taskId);
        }

        private void SetCounterInternal(string taskId, int value)
        {
            storage.Write(taskId, value, globalTime.UpdateNowTicks(), TimeSpan.FromHours(1));
        }

        private IRemoteLock Lock(string taskId)
        {
            return remoteLockCreator.Lock("TestCounterRepository_" + taskId);
        }

        private readonly IGlobalTime globalTime;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly LegacyBlobStorage<int> storage;
    }
}