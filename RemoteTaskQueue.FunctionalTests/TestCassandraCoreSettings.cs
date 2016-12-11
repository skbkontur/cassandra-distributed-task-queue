using SKBKontur.Catalogue.CassandraStorageCore;

namespace RemoteTaskQueue.FunctionalTests
{
    public class TestCassandraCoreSettings : ICassandraCoreSettings
    {
        public int MaximalColumnsCount { get { return 1000; } }
        public int MaximalRowsCount { get { return 1000; } }
        public string KeyspaceName { get { return "QueueKeyspace"; } }
    }
}