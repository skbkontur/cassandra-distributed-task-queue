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
                    TaskId = meta.TaskId,
                    State = meta.State,
                    Name = meta.Name,
                    StartExecutedTicks = meta.StartExecutingTicks.HasValue ? meta.StartExecutingTicks.Value.Ticks.ToString() : "",
                    StartExecutedMoscowTime = meta.StartExecutingTicks.HasValue ? meta.StartExecutingTicks.Value.GetMoscowDateTimeString() : "",
                    EnqueueTicks = meta.Ticks.Ticks.ToString(),
                    EnqueueMoscowTime = meta.Ticks.GetMoscowDateTimeString(),
                    MinimalStartTicks = meta.MinimalStartTicks.Ticks.ToString(),
                    MinimalStartMoscowTime = meta.MinimalStartTicks.GetMoscowDateTimeString(),
                    Attempts = meta.Attempts,
                    ParentTaskId = meta.ParentTaskId,
                    TaskGroupLock = meta.TaskGroupLock
                };
        }
    }
}