using System;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.Core.Web.RenderingHelpers;
using SKBKontur.Catalogue.Expressions;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

using TaskState = SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives.TaskState;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public static class HtmlModelCreatorRemoteTaskQueueExtensions
    {
        public static TaskMetaInfoHtmlModel TaskInfoFor<TData>(this IHtmlModelsCreator<TData> htmlModelsCreator, IPageModel<TData> pageModel, Expression<Func<TData, TaskMetaInfoModel>> path, bool hideTicks, Func<TaskIdHtmlModel> createEmptyModel) where TData : ModelData
        {
            var pathToTaskId = path.Merge(x => x.TaskId);
            return new TaskMetaInfoHtmlModel
                {
                    TaskId = htmlModelsCreator.TaskIdFor(pageModel, pathToTaskId, createEmptyModel),
                    TaskState = htmlModelsCreator.TaskStateFor(pageModel, path.Merge(x => x.State), htmlModelsCreator.GetValue(pageModel.Data, pathToTaskId)),
                    TaskName = htmlModelsCreator.TextFor(pageModel, path.Merge(x => x.Name)),
                    EnqueueTime = htmlModelsCreator.TaskDateTimeFor(pageModel, path.Merge(x => x.EnqueueTime), hideTicks),
                    StartExecutingTime = htmlModelsCreator.TaskDateTimeFor(pageModel, path.Merge(x => x.StartExecutingTime), hideTicks),
                    FinishExecutingTime = htmlModelsCreator.TaskDateTimeFor(pageModel, path.Merge(x => x.FinishExecutingTime), hideTicks),
                    MinimalStartTime = htmlModelsCreator.TaskDateTimeFor(pageModel, path.Merge(x => x.MinimalStartTime), hideTicks),
                    Attempts = htmlModelsCreator.TextFor(pageModel, path.Merge(x => x.Attempts)),
                    ParentTaskId = htmlModelsCreator.TaskIdFor(pageModel, path.Merge(x => x.ParentTaskId), createEmptyModel),
                    TaskGroupLock = htmlModelsCreator.TextFor(pageModel, path.Merge(x => x.TaskGroupLock))
                };
        }

        public static TaskIdHtmlModel TaskIdFor<TData>(this IHtmlModelsCreator<TData> htmlModelsCreator, IPageModel<TData> pageModel, Expression<Func<TData, string>> path, Func<TaskIdHtmlModel> createEmptyModel) where TData : ModelData
        {
            var result = createEmptyModel();
            result.Value = htmlModelsCreator.GetValue(pageModel.Data, path);
            result.Id = htmlModelsCreator.GetName(path).ToId();
            return result;
        }

        public static ExceptionInfoHtmlModel ExceptionInfoFor<TData>(this IHtmlModelsCreator<TData> htmlModelsCreator, IPageModel<TData> pageModel, Expression<Func<TData, TaskExceptionInfo>> path) where TData : ModelData
        {
            var taskExceptionInfo = htmlModelsCreator.GetValue(pageModel.Data, path);
            if(taskExceptionInfo == null)
                return null;
            return new ExceptionInfoHtmlModel
                {
                    Id = htmlModelsCreator.GetName(path).ToId(),
                    ExceptionMessageInfo = taskExceptionInfo.ExceptionMessageInfo
                };
        }

        private static TaskStateHtmlModel TaskStateFor<TData>(this IHtmlModelsCreator<TData> htmlModelsCreator, IPageModel<TData> pageModel, Expression<Func<TData, TaskState>> path, string taskId) where TData : ModelData
        {
            return new TaskStateHtmlModel(taskId)
                {
                    Id = htmlModelsCreator.GetName(path).ToId(),
                    Value = htmlModelsCreator.GetValue(pageModel.Data, path)
                };
        }

        private static TaskDateTimeHtmlModel TaskDateTimeFor<TData>(this IHtmlModelsCreator<TData> htmlModelsCreator, IPageModel<TData> pageModel, Expression<Func<TData, DateTime?>> path, bool hideTicks) where TData : ModelData
        {
            var dateTime = htmlModelsCreator.GetValue(pageModel.Data, path);
            return new TaskDateTimeHtmlModel
                {
                    Id = htmlModelsCreator.GetName(path).ToId(),
                    HideTicks = hideTicks,
                    Ticks = dateTime == null ? (long?)null : dateTime.Value.Ticks,
                    DateTime = dateTime.GetMoscowDateTimeString()
                };
        }
    }
}