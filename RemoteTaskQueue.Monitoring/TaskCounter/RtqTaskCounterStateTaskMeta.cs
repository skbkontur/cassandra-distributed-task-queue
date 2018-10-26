using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.Monitoring.TaskCounter
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

        public override string ToString()
        {
            return $"Name: {Name}, State: {State}, MinimalStartTimestamp: {MinimalStartTimestamp}, LastModificationTicks: {LastModificationTicks}";
        }
    }
}