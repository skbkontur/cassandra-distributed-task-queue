using System;
using System.Linq;

using SKBKontur.Catalogue.Core.ObjectTreeWebViewer.ModelBuilders;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskDetails;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskDetails
{
    public class TaskDetailsHtmlModelBuilder : ITaskDetailsHtmlModelBuilder
    {
        public TaskDetailsHtmlModelBuilder(IObjectTreeValueModelBuilder objectTreeValueModelBuilder, IRemoteTaskQueueHtmlModelsCreator<TaskDetailsModel> htmlModelCreator)
        {
            this.objectTreeValueModelBuilder = objectTreeValueModelBuilder;
            this.htmlModelCreator = htmlModelCreator;
        }

        public TaskDetailsHtmlModel Build(TaskDetailsPageModel pageModel)
        {
            Func<TaskIdHtmlModel> createEmptyTaskIdModel = () => new TaskIdHtmlModel(pageModel.PageNumber, pageModel.SearchRequestId);
            return new TaskDetailsHtmlModel
                {
                    TaskMetaInfo = htmlModelCreator.TaskInfoFor(pageModel, data => data.TaskMetaInfoModel, false, createEmptyTaskIdModel),
                    ChildTaskIds = pageModel.Data.ChildTaskIds.Select((id, i) => htmlModelCreator.TaskIdFor(pageModel, data => data.ChildTaskIds[i], createEmptyTaskIdModel)).ToArray(),
                    TaskDataValue = objectTreeValueModelBuilder.Build(pageModel.Data.TaskData, false, (url, path) => url.GetTaskDataBytesUrl(pageModel.Data.TaskMetaInfoModel.TaskId, path)),
                    ExceptionInfo = htmlModelCreator.ExceptionInfoFor(pageModel, data => data.ExceptionInfo)
                };
        }

        private readonly IObjectTreeValueModelBuilder objectTreeValueModelBuilder;
        private readonly IRemoteTaskQueueHtmlModelsCreator<TaskDetailsModel> htmlModelCreator;
    }
}