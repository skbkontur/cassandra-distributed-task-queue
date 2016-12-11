using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas
{
    [TaskName("ByteArrayTaskData")]
    public class ByteArrayTaskData : ITaskData
    {
        public byte[] Bytes { get; set; }
    }
}