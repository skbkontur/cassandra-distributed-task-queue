using JetBrains.Annotations;

namespace RemoteQueue.Cassandra.Entities
{
    public class Task
    {
        public Task([NotNull] TaskMetaInformation meta, [NotNull] byte[] data)
        {
            Meta = meta;
            Data = data;
        }

        [NotNull]
        public TaskMetaInformation Meta { get; private set; }

        [NotNull]
        public byte[] Data { get; private set; }

        internal bool NeedProlongation()
        {
            return Meta.NeedProlongation();
        }
    }
}