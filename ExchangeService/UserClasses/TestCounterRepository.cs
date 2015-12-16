using System;

using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace ExchangeService.UserClasses
{
    public class TestCounterRepository : ITestCounterRepository
    {
        public TestCounterRepository(LegacyBlobStorage<int> storage, IRemoteLockCreator remoteLockCreator)
        {
            this.storage = storage;
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
            storage.Write(taskId, value, DateTime.UtcNow.Ticks);
        }

        private IRemoteLock Lock(string taskId)
        {
            return remoteLockCreator.Lock("TestCounterRepository_" + taskId);
        }

        private readonly LegacyBlobStorage<int> storage;
        private readonly IRemoteLockCreator remoteLockCreator;
    }
}