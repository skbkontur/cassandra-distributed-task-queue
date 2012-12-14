using System.Collections.Generic;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IMonitoringServiceClient
    {
        void ActualizeDatabaseScheme();
        void DropLocalStorage();

        IEnumerable<MonitoringTaskMetadata> Search(ExpressionTree criterion, ExpressionTree sortRules, int count = 1000, int rangeFrom = 0);
        IEnumerable<object> GetDistinctValues(ExpressionTree criterion, ExpressionTree columnPath);
    }
}