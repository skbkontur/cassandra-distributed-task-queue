using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    public class ByteArrayTaskData : ITaskData
    {
        public string QueueId { get; set; }
        public byte[] Bytes { get; set; }
    }
}