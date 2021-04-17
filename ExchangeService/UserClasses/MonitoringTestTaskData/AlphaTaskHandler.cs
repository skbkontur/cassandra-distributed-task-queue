using RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace ExchangeService.UserClasses.MonitoringTestTaskData
{
    public class AlphaTaskHandler : RtqTaskHandler<AlphaTaskData>
    {
        protected override HandleResult HandleTask(AlphaTaskData taskData)
        {
            return Finish();
        }
    }
}