using System;
using System.Linq.Expressions;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public interface IMonitoringSearchRequestCriterionBuilder
    {
        Expression<Func<TaskMetaInformationBusinessObjectWrap, bool>> BuildCriterion(MonitoringSearchRequest monitoringSearchRequest);
        Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b);
        Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b);
    }
}