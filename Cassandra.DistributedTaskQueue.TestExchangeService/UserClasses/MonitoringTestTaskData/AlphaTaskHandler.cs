using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestExchangeService.UserClasses.MonitoringTestTaskData
{
    public class AlphaTaskHandler : RtqTaskHandler<AlphaTaskData>
    {
        protected override HandleResult HandleTask(AlphaTaskData taskData)
        {
            return Finish();
        }
    }
}