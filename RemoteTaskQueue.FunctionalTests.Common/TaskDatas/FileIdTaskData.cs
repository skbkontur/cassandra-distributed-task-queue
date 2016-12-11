using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas
{
    [TaskName("FileIdTaskData")]
    public class FileIdTaskData : ITaskData
    {
        public string FileId { get; set; }
    }
}