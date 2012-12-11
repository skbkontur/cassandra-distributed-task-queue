using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatasAndHandlers.TaskDatas
{
    public class FakePeriodicTaskData : ITaskData
    {
        public string QueueId { get; set; }
    }
}