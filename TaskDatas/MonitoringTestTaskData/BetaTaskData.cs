using RemoteQueue.Configuration;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData
{
    [TaskName("BetaTaskData")]
    public class BetaTaskData : ITaskDataWithTopic
    {
        public bool IsProcess { get; set; }
    }
}