using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    [TaskName("ByteArrayTaskData")]
    public class ByteArrayTaskData : ITaskData
    {
        public byte[] Bytes { get; set; }
    }
}