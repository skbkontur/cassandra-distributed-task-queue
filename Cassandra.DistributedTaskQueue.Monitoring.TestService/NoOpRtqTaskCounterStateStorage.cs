using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService
{
    public class NoOpRtqTaskCounterStateStorage : IRtqTaskCounterStateStorage
    {
        public byte[] TryRead()
        {
            return null;
        }

        public void Write(byte[] serializedState)
        {
        }
    }
}