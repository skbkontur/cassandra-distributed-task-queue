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

                    EnqueueTicks = meta.Ticks.Ticks.ToString(),
                    MinimalStartTicks = meta.MinimalStartTicks.Ticks.ToString(),
                    StartExecutingTicks = meta.StartExecutingTicks.HasValue ? meta.StartExecutingTicks.Value.Ticks.ToString() : "",
                    FinishExecutingTicks = meta.FinishExecutingTicks.HasValue ? meta.FinishExecutingTicks.Value.Ticks.ToString() : "",
                    
                    EnqueueMoscowTime = meta.Ticks.GetMoscowDateTimeString(),
                    MinimalStartMoscowTime = meta.MinimalStartTicks.GetMoscowDateTimeString(),
                    StartExecutingMoscowTime = meta.StartExecutingTicks.HasValue ? meta.StartExecutingTicks.Value.GetMoscowDateTimeString() : "",
                    FinishExecutingMoscowTime = meta.FinishExecutingTicks.HasValue ? meta.FinishExecutingTicks.Value.GetMoscowDateTimeString() : "",

                    ParentTaskId = meta.ParentTaskId,
                    TaskGroupLock = meta.TaskGroupLock
                };
        }
    }
}