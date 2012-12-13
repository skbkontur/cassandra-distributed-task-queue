using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    public class FakeFailTaskData : ITaskData
    {
        public string QueueId { get; set; }
    }
}