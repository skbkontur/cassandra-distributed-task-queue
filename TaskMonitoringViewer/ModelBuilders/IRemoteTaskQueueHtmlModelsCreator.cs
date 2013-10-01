using System;
using System.Linq.Expressions;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.Web.Models.HtmlModels;
using SKBKontur.Catalogue.Core.Web.PageModels;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public interface IRemoteTaskQueueHtmlModelsCreator<TData> : IHtmlModelsCreator<TData> where TData : ModelData
    {
        TaskMetaInfoHtmlModel TaskInfoFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskMetaInfoModel>> pathz, bool hideTicks, Func<TaskIdHtmlModel> createEmptyModel);
        TaskIdHtmlModel TaskIdFor(IPageModel<TData> pageModel, Expression<Func<TData, string>> path, Func<TaskIdHtmlModel> createEmptyModel);
        ExceptionInfoHtmlModel ExceptionInfoFor(IPageModel<TData> pageModel, Expression<Func<TData, TaskExceptionInfo>> path);
    }
}