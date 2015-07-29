using System;

using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas.MonitoringTestTaskData
{
    public class FailingTaskData : ITaskData
    {
        public Guid UniqueData { get; set; }

        public override string ToString()
        {
            return string.Format("UniqueData: {0}", UniqueData);
        }
    }
}