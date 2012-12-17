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
            if(!TryConvertTaskMetaInformationToMonitoringTaskMetadata(remoteTaskInfo.Context, out metadata))
                throw new Exception("не смог сконвертировать TaskState(RemouteTaskQueue) к TaskState(MonitoringDataTypes)");
            return new TaskViewModel
                {
                    TaskMetaInfoModel = taskMetadataModelBuilder.Build(metadata),
                    TaskDataValue = taskDataModelBuilder.Build(remoteTaskInfo.Context.Id, remoteTaskInfo.TaskData),
                    PageNumber = pageNumber ?? 0,
                    SearchRequestId = searchRequestId
                };
        }

        private bool TryConvertTaskMetaInformationToMonitoringTaskMetadata(TaskMetaInformation info, out MonitoringTaskMetadata taskMetadata)
        {/*
            taskMetadata = new MonitoringTaskMetadata(
                info.Name,
                info.Id,
                info.Ticks,
                info.MinimalStartTicks,
                info.StartExecutingTicks,
                default(MTaskState),
                info.Attempts,
                info.ParentTaskId);
            */
            taskMetadata = new MonitoringTaskMetadata
            {
                Name = info.Name,
                TaskId = info.Id,
                Ticks = info.Ticks,
                MinimalStartTicks = info.MinimalStartTicks,
                StartExecutingTicks = info.StartExecutingTicks,
                State = default(MTaskState),
                Attempts = info.Attempts,
                ParentTaskId = info.ParentTaskId,
            };

            MTaskState mtaskState;
            if (!Enum.TryParse(info.State.ToString(), true, out mtaskState))
                return false;
            taskMetadata.State = mtaskState;
            return true;
        }

        private readonly ITaskMetadataModelBuilder taskMetadataModelBuilder;
        private readonly ITaskDataModelBuilder taskDataModelBuilder;
    }
}