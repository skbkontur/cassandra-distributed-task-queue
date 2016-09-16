using System;

using GroBuf;

using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace ExchangeService.UserClasses
{
    public class TestCounterRepository : ITestCounterRepository
    {
        public TestCounterRepository(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings taskQueueSettings, IRemoteLockCreator remoteLockCreator)
        {
            storage = new LegacyBlobStorage<int>(cassandraCluster, serializer, taskQueueSettings.QueueKeyspace, CfName);
            this.remoteLockCreator = remoteLockCreator;
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
            try
            {
                return storage.Read(taskId);
            }
            catch
            {
                return 0;
            }
        }

        private void SetCounterInternal(string taskId, int value)
        {
            storage.Write(taskId, value, DateTime.UtcNow.Ticks, TimeSpan.FromHours(1));
        }

        private IRemoteLock Lock(string taskId)
        {
            return remoteLockCreator.Lock("TestCounterRepository_" + taskId);
        }

        public const string CfName = "TestCounterRepositoryCf";

        private readonly LegacyBlobStorage<int> storage;
        private readonly IRemoteLockCreator remoteLockCreator;
    }
}