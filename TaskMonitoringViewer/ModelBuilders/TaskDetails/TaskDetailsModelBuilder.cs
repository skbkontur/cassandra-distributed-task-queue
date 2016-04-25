using System;
using System.Linq;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;

using TaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public class TaskDetailsModelBuilder : ITaskDetailsModelBuilder
    {
        public TaskDetailsModelBuilder(ITaskMetadataModelBuilder taskMetadataModelBuilder, IRemoteTaskQueue remoteTaskQueue)
        {
            this.taskMetadataModelBuilder = taskMetadataModelBuilder;
            this.remoteTaskQueue = remoteTaskQueue;
        }

        public TaskDetailsModel Build(RemoteTaskInfo remoteTaskInfo, int? pageNumber, string searchRequestId)
        {
            MonitoringTaskMetadata metadata;
            TryConvertTaskMetaInformationToMonitoringTaskMetadata(remoteTaskInfo.Context, out metadata);
            return new TaskDetailsModel
                {
                    TaskMetaInfoModel = taskMetadataModelBuilder.Build(metadata),
                    ChildTaskIds = remoteTaskQueue.GetChildrenTaskIds(remoteTaskInfo.Context.Id),
                    TaskData = remoteTaskInfo.TaskData,
                    ExceptionInfo = remoteTaskInfo.ExceptionInfos.LastOrDefault()
                };
        }

        private bool TryConvertTaskMetaInformationToMonitoringTaskMetadata(TaskMetaInformation info, out MonitoringTaskMetadata taskMetadata)
        {
            if(info == null)
            {
                taskMetadata = new MonitoringTaskMetadata();
                return false;
            }
            DateTime minimalStartTicks;
            if(info.MinimalStartTicks < DateTime.MinValue.Ticks)
                minimalStartTicks = DateTime.MinValue;
            else if(info.MinimalStartTicks > DateTime.MaxValue.Ticks)
                minimalStartTicks = DateTime.MaxValue;
            else
                minimalStartTicks = new DateTime(info.MinimalStartTicks);
            taskMetadata = new MonitoringTaskMetadata
                {
                    Name = info.Name,
                    TaskId = info.Id,
                    Ticks = new DateTime(info.Ticks),
                    MinimalStartTicks = minimalStartTicks,
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
        private readonly IRemoteTaskQueue remoteTaskQueue;
    }
}