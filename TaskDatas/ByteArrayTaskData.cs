using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    public class ByteArrayTaskData : ITaskData
    {
        public byte[] Bytes { get; set; }
    }
}