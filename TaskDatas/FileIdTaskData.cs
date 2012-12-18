using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    public class FileIdTaskData : ITaskData
    {
        public string QueueId { get; set; }
        public string FileId { get; set; }
    }
}