using System;
using System.Linq;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public interface ITaskDetailsHtmlModelBuilder
    {
        TaskDetailsHtmlModel Build(TaskDetailsPageModel pageModel);
    }

    public class TaskDetailsHtmlModelBuilder : ITaskDetailsHtmlModelBuilder
    {
        public TaskDetailsHtmlModelBuilder(ITaskDataModelBuilder taskDataModelBuilder, IRemoteTaskQueueHtmlModelCreator<TaskDetailsModel> htmlModelCreator)
        {
            this.taskDataModelBuilder = taskDataModelBuilder;
            this.htmlModelCreator = htmlModelCreator;
        }

        public TaskDetailsHtmlModel Build(TaskDetailsPageModel pageModel)
        {
            Func<TaskIdHtmlModel> createEmptyTaskIdModel = () => new TaskIdHtmlModel(pageModel.PageNumber, pageModel.SearchRequestId);
            return new TaskDetailsHtmlModel
                {
                    TaskMetaInfo = htmlModelCreator.TaskInfoFor(pageModel, data => data.TaskMetaInfoModel, false, createEmptyTaskIdModel),
                    ChildTaskIds = pageModel.Data.ChildTaskIds.Select((id, i) => htmlModelCreator.TaskIdFor(pageModel, data => data.ChildTaskIds[i], createEmptyTaskIdModel)).ToArray(),
                    TaskDataValue = taskDataModelBuilder.Build(pageModel.Data.TaskMetaInfoModel.TaskId, pageModel.Data.TaskData),
                    ExceptionInfo = htmlModelCreator.ExceptionInfoFor(pageModel, data => data.ExceptionInfo)
                };
        }

        private readonly ITaskDataModelBuilder taskDataModelBuilder;
        private readonly IRemoteTaskQueueHtmlModelCreator<TaskDetailsModel> htmlModelCreator;
    }
}