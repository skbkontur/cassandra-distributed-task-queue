using SKBKontur.Catalogue.CassandraStorageCore;

namespace TestCommon
{
    public class TestCassandraCoreSettings : ICassandraCoreSettings
    {
        public int MaximalColumnsCount { get { return 1000; } }
        public int MaximalRowsCount { get { return 1000; } }
        public string KeyspaceName { get { return "QueueKeyspace"; } }
    }
}
