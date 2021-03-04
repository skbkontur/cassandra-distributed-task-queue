using System.Threading;

using JetBrains.Annotations;

using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    [PublicAPI]
    public interface IRtqTaskCounterEventFeeder
    {
        ( /*[NotNull]*/ IEventFeedsRunner, /*[NotNull]*/ RtqTaskCounterStateManager) RunEventFeeding(CancellationToken cancellationToken);
    }
}