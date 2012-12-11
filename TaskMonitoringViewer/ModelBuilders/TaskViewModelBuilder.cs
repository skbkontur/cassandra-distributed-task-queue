using RemoteQueue.Handling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class TaskViewModelBuilder : ITaskViewModelBuilder
    {
        public TaskViewModelBuilder(ITaskMetaInfoModelBuilder taskMetaInfoModelBuilder, ITaskDataModelBuilder taskDataModelBuilder)
        {
            this.taskMetaInfoModelBuilder = taskMetaInfoModelBuilder;
            this.taskDataModelBuilder = taskDataModelBuilder;
        }

        public TaskViewModel Build(RemoteTaskInfo remoteTaskInfo)
        {
            return new TaskViewModel
                {
                    TaskMetaInfoModel = taskMetaInfoModelBuilder.Build(remoteTaskInfo.Context),
                    TaskDataValue = taskDataModelBuilder.Build(remoteTaskInfo.Context.Id, remoteTaskInfo.TaskData),
                };
        }

        private readonly ITaskMetaInfoModelBuilder taskMetaInfoModelBuilder;
        private readonly ITaskDataModelBuilder taskDataModelBuilder;
    }
}