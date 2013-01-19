using System;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Core.Web.ReferencesHelpers;
using SKBKontur.Catalogue.Core.Web.RenderingHelpers;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

using TaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class RemoteTaskQueueHtmlModelCreator<TData> : HtmlModelsCreatorBase<TData>, IRemoteTaskQueueHtmlModelCreator<TData>
        where TData : ModelData
    {
        public RemoteTaskQueueHtmlModelCreator(ISelectModelBuilder selectModelBuilder, IHtmlModelTemplateBuilder htmlModelTemplateBuilder)
            : base(selectModelBuilder, htmlModelTemplateBuilder)
        {
        }

        public TaskMetaInfoHtmlModel TaskInfoFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskMetaInfoModel>> path, bool hideTicks, Func<TaskIdHtmlModel> createEmptyModel)
        {
            var pathToTaskId = path.Merge(x => x.TaskId);
            return new TaskMetaInfoHtmlModel
                {
                    TaskId = TaskIdFor(pageModel, pathToTaskId, createEmptyModel),
                    TaskState = TaskStateFor(pageModel, path.Merge(x => x.State), GetValue(pageModel.Data, pathToTaskId)),
                    TaskName = TextFor(pageModel, path.Merge(x => x.Name)),
                    EnqueueTime = TaskDateTimeFor(pageModel, path.Merge(x => x.EnqueueTime), hideTicks),
                    StartExecutingTime = TaskDateTimeFor(pageModel, path.Merge(x => x.StartExecutingTime), hideTicks),
                    FinishExecutingTime = TaskDateTimeFor(pageModel, path.Merge(x => x.FinishExecutingTime), hideTicks),
                    MinimalStartTime = TaskDateTimeFor(pageModel, path.Merge(x => x.MinimalStartTime), hideTicks),
                    Attempts = TextFor(pageModel, path.Merge(x => x.Attempts)),
                    ParentTaskId = TaskIdFor(pageModel, path.Merge(x => x.ParentTaskId), createEmptyModel),
                    TaskGroupLock = TextFor(pageModel, path.Merge(x => x.TaskGroupLock))
                };
        }

        public TaskIdHtmlModel TaskIdFor(IPageModel<TData> pageModel, Expression<Func<TData, string>> path, Func<TaskIdHtmlModel> createEmptyModel)
        {
            var result = createEmptyModel();
            result.Value = GetValue(pageModel.Data, path);
            result.Id = GetName(path).ToId();
            return result;
        }

        public ExceptionInfoHtmlModel ExceptionInfoFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskExceptionInfo>> path)
        {
            var taskExceptionInfo = GetValue(pageModel.Data, path);
            if(taskExceptionInfo == null)
                return null;
            return new ExceptionInfoHtmlModel
                {
                    Id = GetName(path).ToId(),
                    ExceptionMessageInfo = taskExceptionInfo.ExceptionMessageInfo
                };
        }

        private TaskStateHtmlModel TaskStateFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskState>> path, string taskId)
        {
            return new TaskStateHtmlModel(taskId)
                {
                    Id = GetName(path).ToId(),
                    Value = GetValue(pageModel.Data, path)
                };
        }

        private TaskDateTimeHtmlModel TaskDateTimeFor(IPageModel<TData> pageModel, Expression<Func<TData, DateTime?>> path, bool hideTicks)
        {
            var dateTime = GetValue(pageModel.Data, path);
            return new TaskDateTimeHtmlModel
                {
                    Id = GetName(path).ToId(),
                    HideTicks = hideTicks,
                    Ticks = dateTime == null ? (long?)null : dateTime.Value.Ticks,
                    DateTime = dateTime.GetMoscowDateTimeString()
                };
        }
    }
}