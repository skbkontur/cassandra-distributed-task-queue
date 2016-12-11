using Newtonsoft.Json;

namespace RemoteTaskQueue.Monitoring.Storage.Writing.Contracts
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

        [JsonConverter(typeof(TruncateLongStringsConverter2K))]
        public string ExceptionInfo { get; set; }
    }
}