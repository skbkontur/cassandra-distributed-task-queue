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
                    MinimalStartTicks = meta.MinimalStartTicks.Ticks.ToString(),
                    StartExecutedTicks = meta.StartExecutingTicks.HasValue ? meta.StartExecutingTicks.Value.Ticks.ToString() : "",
                    EnqueueMoscowTime = meta.Ticks.GetMoscowDateTimeString(),
                    MinimalStartMoscowTime = meta.MinimalStartTicks.GetMoscowDateTimeString(),
                    StartExecutedMoscowTime = meta.StartExecutingTicks.HasValue ? meta.StartExecutingTicks.Value.GetMoscowDateTimeString() : "",
                    Attempts = meta.Attempts,
                    ParentTaskId = meta.ParentTaskId
                };
        }
    }
}