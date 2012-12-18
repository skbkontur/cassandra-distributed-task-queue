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
                    EnqueueTicks = meta.Ticks.ToString(),
                    StartExecutedTicks = meta.StartExecutingTicks.ToString(),
                    MinimalStartTicks = meta.MinimalStartTicks.ToString(),
                    Attempts = meta.Attempts,
                    ParentTaskId = meta.ParentTaskId
                };
        }
    }
}