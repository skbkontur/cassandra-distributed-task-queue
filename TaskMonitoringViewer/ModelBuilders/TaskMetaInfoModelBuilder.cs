using System;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class TaskMetaInfoModelBuilder : ITaskMetaInfoModelBuilder
    {
        public TaskMetaInfoModel Build(TaskMetaInformation meta)
        {
            return new TaskMetaInfoModel
                {
                    Id = meta.Id,
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