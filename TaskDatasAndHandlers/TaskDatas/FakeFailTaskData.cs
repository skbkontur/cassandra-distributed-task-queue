using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatasAndHandlers.TaskDatas
{
    public class FakeFailTaskData : ITaskData
    {
        public string QueueId { get; set; }
    }
}