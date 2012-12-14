using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    public class SimpleTaskData : ITaskData
    {
        public string QueueId { get; set; }
    }
}