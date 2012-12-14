using System;

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
                    EnqueueTicks = TicksToDateString(meta.Ticks),
                    StartExecutedTicks = TicksToDateString(meta.StartExecutingTicks),
                    MinimalStartTicks = TicksToDateString(meta.MinimalStartTicks),
                    Attempts = meta.Attempts,
                    ParentTaskId = meta.ParentTaskId
                };
        }

        private string TicksToDateString(long? ticks)
        {
            return ticks == null ? null : new DateTime(ticks.Value).ToString();
        }
    }
}