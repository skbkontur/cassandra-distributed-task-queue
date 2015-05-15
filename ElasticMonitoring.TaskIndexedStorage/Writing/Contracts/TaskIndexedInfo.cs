namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts
{
    public class TaskIndexedInfo
    {
        public MetaIndexedInfo Meta { get; set; }
        public object Data { get; set; }
    }
}