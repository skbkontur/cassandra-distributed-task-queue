using RemoteQueue.Cassandra.RemoteLock;

namespace ExchangeService.UserClasses
{
    public class TestCounterRepository : ITestCounterRepository
    {
        public TestCounterRepository(ITestCassandraCounterBlobRepository storage, IRemoteLockCreator remoteLockCreator)
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
            storage.Write(taskId, value);
        }

        private IRemoteLock Lock(string taskId)
        {
            return remoteLockCreator.Lock("TestCounterRepository_" + taskId);
        }

        private readonly ITestCassandraCounterBlobRepository storage;
        private readonly IRemoteLockCreator remoteLockCreator;
    }
}