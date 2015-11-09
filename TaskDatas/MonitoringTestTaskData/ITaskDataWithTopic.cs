using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData
{
    [TaskTopic("TestTopic")]
    public interface ITaskDataWithTopic : ITaskData
    {
    }
}