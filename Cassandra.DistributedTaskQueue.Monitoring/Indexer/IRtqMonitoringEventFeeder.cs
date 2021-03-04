using System.Threading;

using JetBrains.Annotations;

using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    [PublicAPI]
    public interface IRtqMonitoringEventFeeder
    {
        [NotNull]
        IEventFeedsRunner RunEventFeeding(CancellationToken cancellationToken);
    }
}