namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts
{
    public class TaskIndexedInfo<T>
    {
        public TaskIndexedInfo()
        {
        }

        public TaskIndexedInfo(MetaIndexedInfo meta, T data)
        {
            Meta = meta;
            Data = data;
        }

        public MetaIndexedInfo Meta { get; set; }
        public T Data { get; set; }
    }
}