using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [PublicAPI]
    public interface IRtqConsumer
    {
        void Start();
        void Stop();
    }
}