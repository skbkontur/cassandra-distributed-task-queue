using SKBKontur.Catalogue.Expressions.ExpressionTrees;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.Queries
{
    public class MonitoringGetDistinctValuesQuery
    {
        public ExpressionTree Criterion { get; set; }
        public ExpressionTree ColumnPath { get; set; }
    }
}