using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    public class FakePeriodicTaskData : ITaskData
    {
        public string QueueId { get; set; }
    }
}