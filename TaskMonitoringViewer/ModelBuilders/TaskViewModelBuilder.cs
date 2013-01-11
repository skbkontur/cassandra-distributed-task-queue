using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

using MTaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class TaskViewModelBuilder : ITaskViewModelBuilder
    {
        public TaskViewModelBuilder(ITaskMetadataModelBuilder taskMetadataModelBuilder, ITaskDataModelBuilder taskDataModelBuilder)
        {
            this.taskMetadataModelBuilder = taskMetadataModelBuilder;
            this.taskDataModelBuilder = taskDataModelBuilder;
        }

        public TaskViewModel Build(RemoteTaskInfo remoteTaskInfo, int? pageNumber, string searchRequestId)
        {
            MonitoringTaskMetadata metadata;
            TryConvertTaskMetaInformationToMonitoringTaskMetadata(remoteTaskInfo.Context, out metadata);
            return new TaskViewModel
                {
                    TaskMetaInfoModel = taskMetadataModelBuilder.Build(metadata),
                    TaskDataValue = taskDataModelBuilder.Build(remoteTaskInfo.Context.Id, remoteTaskInfo.TaskData),
                    PageNumber = pageNumber ?? 0,
                    SearchRequestId = searchRequestId,
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
                    State = default(MTaskState),
                    Attempts = info.Attempts,
                    TaskGroupLock = info.TaskGroupLock,
                    ParentTaskId = info.ParentTaskId,
                };

            MTaskState mtaskState;
            if(!Enum.TryParse(info.State.ToString(), true, out mtaskState))
                return false;
            taskMetadata.State = mtaskState;
            return true;
        }

        private readonly ITaskMetadataModelBuilder taskMetadataModelBuilder;
        private readonly ITaskDataModelBuilder taskDataModelBuilder;
    }
}