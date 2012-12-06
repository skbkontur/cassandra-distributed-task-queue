using RemoteQueue.Handling;

namespace ExchangeService.TaskDatas
{
    public class FakePeriodicTaskData : ITaskData
    {
        public string QueueId { get; set; }
    }
}