namespace RemoteTaskQueue.Monitoring.Storage
{
    public static class RtqElasticsearchConsts
    {
        public const string AllIndicesWildcard = "rtq-*";
        public const string TemplateName = "rtq-template";
        public const string DataIndexNameFormat = @"\r\t\q-yyyy.MM.dd";
        public const string IndexingProgressIndexName = "rtq-indexingprogress";
    }
}