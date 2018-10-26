using RemoteTaskQueue.Monitoring.TaskCounter;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService
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