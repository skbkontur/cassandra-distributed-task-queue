namespace RemoteTaskQueue.Monitoring.Storage
{
    public static class RtqElasticsearchConsts
    {
        public const string IndexPrefix = "rtq-";
        public const string TemplateName = "rtq-template";
        public const string CurrentIndexNameFormat = "rtq-{yyyy.MM.dd}";
        public const string OldDataIndex = "rtq-olddata";
        public const string OldDataAliasFormat = "{index}-old";
        public const string SearchAliasFormat = "{index}-search";
        public const string IndexingProgressIndex = "rtq-indexingprogress";
    }
}