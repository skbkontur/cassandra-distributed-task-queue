using System;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;

using TaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public class TaskDetailsModelBuilder : ITaskDetailsModelBuilder
    {
        public TaskDetailsModelBuilder(
            ITaskMetadataModelBuilder taskMetadataModelBuilder,
            ITaskDataModelBuilder taskDataModelBuilder,
            IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage)
        {
            this.taskMetadataModelBuilder = taskMetadataModelBuilder;
            this.taskDataModelBuilder = taskDataModelBuilder;
            this.remoteTaskQueueMonitoringServiceStorage = remoteTaskQueueMonitoringServiceStorage;
        }

        public TaskDetailsModel Build(RemoteTaskInfo remoteTaskInfo, int? pageNumber, string searchRequestId)
        {
            MonitoringTaskMetadata metadata;
            TryConvertTaskMetaInformationToMonitoringTaskMetadata(remoteTaskInfo.Context, out metadata);

            return new TaskDetailsModel
                {
                    TaskMetaInfoModel = taskMetadataModelBuilder.Build(metadata),
                    ChildTaskIds = remoteTaskQueueMonitoringServiceStorage.RangeSearch(x => x.ParentTaskId == remoteTaskInfo.Context.Id, 0, 1000).Select(x => x.Id).ToArray(),
                    TaskData = remoteTaskInfo.TaskData,
                    ExceptionInfo = remoteTaskInfo.ExceptionInfo
                };
        }

        private bool TryConvertTaskMetaInformationToMonitoringTaskMetadata(TaskMetaInformation info, out MonitoringTaskMetadata taskMetadata)
        {
            if(info == null)
            {
                taskMetadata = new MonitoringTaskMetadata();
                return false;
            }
            taskMetadata = new MonitoringTaskMetadata
                {
                    Name = info.Name,
                    TaskId = info.Id,
                    Ticks = new DateTime(info.Ticks),
                    MinimalStartTicks = new DateTime(info.MinimalStartTicks),
                    StartExecutingTicks = info.StartExecutingTicks.HasValue ? (DateTime?)new DateTime(info.StartExecutingTicks.Value) : null,
                    FinishExecutingTicks = info.FinishExecutingTicks.HasValue ? (DateTime?)new DateTime(info.FinishExecutingTicks.Value) : null,
                    State = default(TaskState),
                    Attempts = info.Attempts,
                    TaskGroupLock = info.TaskGroupLock,
                    ParentTaskId = info.ParentTaskId,
                };

            TaskState mtaskState;
            if(!Enum.TryParse(info.State.ToString(), true, out mtaskState))
                return false;
            taskMetadata.State = mtaskState;
            return true;
        }

        private readonly ITaskMetadataModelBuilder taskMetadataModelBuilder;
        private readonly ITaskDataModelBuilder taskDataModelBuilder;
        private readonly IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage;
    }
}