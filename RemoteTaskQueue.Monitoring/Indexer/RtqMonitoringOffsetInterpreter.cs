using JetBrains.Annotations;

using RemoteQueue.Cassandra.Repositories;

using SKBKontur.Catalogue.Core.EventFeeds;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class RtqMonitoringOffsetInterpreter : IOffsetInterpreter<string>
    {
        [NotNull]
        public string Format([CanBeNull] string offset)
        {
            return string.Format("EventPointer: {0}, Timestamp: {1}", offset, GetTimestampFromOffset(offset));
        }

        [CanBeNull]
        public Timestamp GetTimestampFromOffset([CanBeNull] string offset)
        {
            return string.IsNullOrEmpty(offset) ? null : EventPointerFormatter.GetTimestamp(offset);
        }

        [NotNull]
        public string GetMaxOffsetForTimestamp([NotNull] Timestamp timestamp)
        {
            return EventPointerFormatter.GetColumnName(timestamp.Ticks, GuidHelpers.MaxGuid);
        }

        public int Compare([CanBeNull] string x, [CanBeNull] string y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            return EventPointerFormatter.CompareColumnNames(x, y);
        }
    }
}