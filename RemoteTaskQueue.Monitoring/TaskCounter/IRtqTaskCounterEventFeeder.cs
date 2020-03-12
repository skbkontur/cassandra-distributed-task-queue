using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    [PublicAPI]
    public interface IRtqTaskCounterEventFeeder
    {
        ( /*[NotNull]*/ IEventFeedsRunner, /*[NotNull]*/ RtqTaskCounterStateManager) RunEventFeeding();
    }
}