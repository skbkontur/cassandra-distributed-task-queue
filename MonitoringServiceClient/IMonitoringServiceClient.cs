using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Expressions.ExpressionTrees;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient
{
    public interface IMonitoringServiceClient
    {
        void ActualizeDatabaseScheme();
        void DropLocalStorage();

        TaskMetaInformation[] Search(ExpressionTree criterion, ExpressionTree sortRules, int count = 1000, int rangeFrom = 0);
        object[] GetDistinctValues(ExpressionTree criterion, ExpressionTree columnPath);
    }
}