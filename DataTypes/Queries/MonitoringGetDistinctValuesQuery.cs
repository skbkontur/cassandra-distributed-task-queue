using SKBKontur.Catalogue.Expressions.ExpressionTrees;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries
{
    public class MonitoringGetDistinctValuesQuery
    {
        public ExpressionTree Criterion { get; set; }
        public ExpressionTree ColumnPath { get; set; }
    }
}