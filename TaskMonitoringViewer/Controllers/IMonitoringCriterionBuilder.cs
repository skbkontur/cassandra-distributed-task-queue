using System;
using System.Linq.Expressions;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Controllers
{
    public interface IMonitoringSearchRequestCriterionBuilder
    {
        Expression<Func<MonitoringTaskMetadata, bool>> BuildCriterion(MonitoringSearchRequest monitoringSearchRequest);
    }
}