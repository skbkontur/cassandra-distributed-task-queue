namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Writing
{
    public class TruncateLongStringsConverter2K : TruncateLongStringsConverter
    {
        public TruncateLongStringsConverter2K()
            : base(2048)
        {
        }
    }
}