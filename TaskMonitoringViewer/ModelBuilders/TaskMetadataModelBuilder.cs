using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class TaskMetadataModelBuilder : ITaskMetadataModelBuilder
    {
        public TaskMetaInfoModel Build(MonitoringTaskMetadata meta)
        {
            return new TaskMetaInfoModel
                {
                    Attempts = meta.Attempts,
                    TaskId = meta.TaskId,
                    Name = meta.Name,
                    State = meta.State,

                    EnqueueTime = meta.Ticks,
                    MinimalStartTime = meta.MinimalStartTicks,
                    StartExecutingTime = meta.StartExecutingTicks,
                    FinishExecutingTime = meta.FinishExecutingTicks,

                    ParentTaskId = meta.ParentTaskId,
                    TaskGroupLock = meta.TaskGroupLock
                };
        }
    }
}