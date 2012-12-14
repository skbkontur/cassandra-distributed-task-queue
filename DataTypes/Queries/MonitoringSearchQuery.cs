using SKBKontur.Catalogue.Expressions.ExpressionTrees;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.Queries
{
    public class MonitoringSearchQuery
    {
        public ExpressionTree SortRules { get; set; }
        public ExpressionTree Criterion { get; set; }
        public int RangeFrom { get; set; }
        public int Count { get; set; }
    }
}