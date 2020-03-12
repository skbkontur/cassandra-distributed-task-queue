using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    [PublicAPI]
    public interface IRtqMonitoringEventFeeder
    {
        [NotNull]
        IEventFeedsRunner RunEventFeeding();
    }
}