namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    internal class TruncateLongStringsConverter2K : TruncateLongStringsConverter
    {
        public TruncateLongStringsConverter2K()
            : base(2048)
        {
        }
    }
}