namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
{
    internal class TruncateLongStringsConverter2K : TruncateLongStringsConverter
    {
        public TruncateLongStringsConverter2K()
            : base(2048)
        {
        }
    }
}