using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounterStateTaskMeta
    {
        public RtqTaskCounterStateTaskMeta([NotNull] string name)
        {
            Name = name;
        }

        [NotNull]
        public string Name { get; }

        public TaskState State { get; set; }

        [NotNull]
        public Timestamp MinimalStartTimestamp { get; set; }

        public long? LastModificationTicks { get; set; }

        [NotNull]
        public Timestamp LastStateUpdateTimestamp { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, State: {State}, MinimalStartTimestamp: {MinimalStartTimestamp}, LastModificationTicks: {LastModificationTicks}, LastStateUpdateTimestamp: {LastStateUpdateTimestamp}";
        }
    }
}