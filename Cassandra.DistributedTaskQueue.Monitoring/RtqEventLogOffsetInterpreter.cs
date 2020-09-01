using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.EventFeeds;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring
{
    public class RtqEventLogOffsetInterpreter : IOffsetInterpreter<string>
    {
        [NotNull]
        public string Format([CanBeNull] string offset)
        {
            return $"EventPointer: {offset}, Timestamp: {GetTimestampFromOffset(offset)}";
        }

        [CanBeNull]
        public Timestamp GetTimestampFromOffset([CanBeNull] string offset)
        {
            return string.IsNullOrEmpty(offset) ? null : EventPointerFormatter.GetTimestamp(offset);
        }

        [NotNull]
        public string GetMaxOffsetForTimestamp([NotNull] Timestamp timestamp)
        {
            return EventPointerFormatter.GetMaxColumnNameForTimestamp(timestamp);
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