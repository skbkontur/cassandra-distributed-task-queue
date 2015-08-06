using Newtonsoft.Json;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing.Contracts
{
    public class TaskIndexedInfo<T>
    {
        public TaskIndexedInfo()
        {
        }

        public TaskIndexedInfo(MetaIndexedInfo meta, string exceptionInfo, T data)
        {
            Meta = meta;
            ExceptionInfo = exceptionInfo;
            Data = data;
        }

        public MetaIndexedInfo Meta { get; set; }
        public T Data { get; set; }

        [JsonConverter(typeof(StringConverter))]
        public string ExceptionInfo { get; set; }
    }
}