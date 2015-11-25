using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    [TaskName("FileIdTaskData")]
    public class FileIdTaskData : ITaskData
    {
        public string FileId { get; set; }
    }
}