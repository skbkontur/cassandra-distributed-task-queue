using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
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

                    EnqueueTime = DateAndTime.Create(meta.Ticks, TimeZoneId.MoscowTimeZoneId),
                    MinimalStartTime = DateAndTime.Create(meta.MinimalStartTicks, TimeZoneId.MoscowTimeZoneId),
                    StartExecutingTime = DateAndTime.Create(meta.StartExecutingTicks, TimeZoneId.MoscowTimeZoneId),
                    FinishExecutingTime = DateAndTime.Create(meta.FinishExecutingTicks, TimeZoneId.MoscowTimeZoneId),

                    ParentTaskId = meta.ParentTaskId,
                    TaskGroupLock = meta.TaskGroupLock
                };
        }
    }
}