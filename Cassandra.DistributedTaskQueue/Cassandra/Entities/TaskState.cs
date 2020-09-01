namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities
{
    public enum TaskState
    {
        [CassandraName("Unknown")]
        Unknown = 0,

        [CassandraName("New")]
        New,

        [CassandraName("WaitingForRerun")]
        WaitingForRerun,

        [CassandraName("WaitingForRerunAfterError")]
        WaitingForRerunAfterError,

        [CassandraName("Finished")]
        Finished,

        [CassandraName("InProcess")]
        InProcess,

        [CassandraName("Fatal")]
        Fatal,

        [CassandraName("Canceled")]
        Canceled,
    }
}