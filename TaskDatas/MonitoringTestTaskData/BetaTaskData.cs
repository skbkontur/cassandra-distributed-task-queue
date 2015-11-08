using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData
{
    [TaskName("BetaTaskData")]
    public class BetaTaskData : ITaskData
    {
        public bool IsProcess { get; set; }
    }
}