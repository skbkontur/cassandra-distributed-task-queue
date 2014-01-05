using System;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Core.Web.RenderingHelpers;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

using TaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public class RemoteTaskQueueHtmlModelsCreator<TData> : HtmlModelsCreatorBase<TData>, IRemoteTaskQueueHtmlModelsCreator<TData>
        where TData : ModelData
    {
        public RemoteTaskQueueHtmlModelsCreator(HtmlModelCreatorParameters parameters)
            : base(parameters)
        {
        }

        public TaskMetaInfoHtmlModel TaskInfoFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskMetaInfoModel>> pathz, bool hideTicks, Func<TaskIdHtmlModel> createEmptyModel)
        {
            var simplifiedPath = pathz.ToSimplifiedExpression();
            var pathToTaskId = simplifiedPath.Merge(x => x.TaskId);
            return new TaskMetaInfoHtmlModel
                {
                    TaskId = TaskIdFor(pageModel, pathToTaskId, createEmptyModel),
                    TaskState = TaskStateFor(pageModel, simplifiedPath.Merge(x => x.State), GetValue(pageModel.Data, pathToTaskId)),
                    TaskName = TextFor(pageModel, simplifiedPath.Merge(x => x.Name)),
                    EnqueueTime = TaskDateTimeFor(pageModel, simplifiedPath.Merge(x => x.EnqueueTime), hideTicks),
                    StartExecutingTime = TaskDateTimeFor(pageModel, simplifiedPath.Merge(x => x.StartExecutingTime), hideTicks),
                    FinishExecutingTime = TaskDateTimeFor(pageModel, simplifiedPath.Merge(x => x.FinishExecutingTime), hideTicks),
                    MinimalStartTime = TaskDateTimeFor(pageModel, simplifiedPath.Merge(x => x.MinimalStartTime), hideTicks),
                    Attempts = TextFor(pageModel, simplifiedPath.Merge(x => x.Attempts)),
                    ParentTaskId = TaskIdFor(pageModel, simplifiedPath.Merge(x => x.ParentTaskId), createEmptyModel),
                    TaskGroupLock = TextFor(pageModel, simplifiedPath.Merge(x => x.TaskGroupLock))
                };
        }

        public TaskIdHtmlModel TaskIdFor(IPageModel<TData> pageModel, Expression<Func<TData, string>> path, Func<TaskIdHtmlModel> createEmptyModel)
        {
            return TaskIdFor(pageModel, path.ToSimplifiedExpression(), createEmptyModel);
        }

        public ExceptionInfoHtmlModel ExceptionInfoFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskExceptionInfo>> path)
        {
            var simplifiedPath = path.ToSimplifiedExpression();
            var name = GetName(simplifiedPath);
            var taskExceptionInfo = GetValue<TaskExceptionInfo>(pageModel.Data, name);
            if(taskExceptionInfo == null)
                return null;
            return new ExceptionInfoHtmlModel
                {
                    Id = name.ToId(),
                    ExceptionMessageInfo = taskExceptionInfo.ExceptionMessageInfo
                };
        }

        private TaskIdHtmlModel TaskIdFor(IPageModel<TData> pageModel, SimplifiedExpression<Func<TData, string>> path, Func<TaskIdHtmlModel> createEmptyModel)
        {
            var name = GetName(path);
            var result = createEmptyModel();
            result.Value = GetValue<string>(pageModel.Data, name);
            result.Id = name.ToId();
            return result;
        }

        private TaskStateHtmlModel TaskStateFor(IPageModel<TData> pageModel, SimplifiedExpression<Func<TData, TaskState>> path, string taskId)
        {
            var name = GetName(path);
            return new TaskStateHtmlModel(taskId)
                {
                    Id = name.ToId(),
                    Value = GetValue<TaskState>(pageModel.Data, name)
                };
        }

        private TaskDateTimeHtmlModel TaskDateTimeFor(IPageModel<TData> pageModel, SimplifiedExpression<Func<TData, DateAndTime>> path, bool hideTicks)
        {
            var name = GetName(path);
            var dateTime = DateAndTime.ToDateTime(GetValue<DateAndTime>(pageModel.Data, name));
            return new TaskDateTimeHtmlModel
                {
                    Id = name.ToId(),
                    HideTicks = hideTicks,
                    Ticks = !dateTime.HasValue ? (long?)null : dateTime.Value.Ticks,
                    DateTime = ReadonlyDateAndTimeFor(pageModel, path, new ReadonlyDateAndTimeOptions{TimeFormat = TimeFormat.Long})//dateTime.GetMoscowDateTimeString()
                };
        }
    }
}