using System.Linq;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl
{
    public class TestTaskLogger : ColumnFamilyRepositoryBase, ITestTaskLogger
    {
        public TestTaskLogger(ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings taskQueueSettings)
            :
            base(cassandraCluster, taskQueueSettings, ColumnFamilies.TestTaskLoggerCfName)
        {
        }

        public void Log(string loggingTaskIdKey, string taskId)
        {
            RetrieveColumnFamilyConnection().AddColumn(loggingTaskIdKey, new Column
                {
                    Name = taskId,
                    Value = new byte[] {1},
                    Timestamp = Timestamp.Now.Ticks
                });
        }

        public string[] GetAll(string loggingTaskIdKey)
        {
            return RetrieveColumnFamilyConnection().GetRow(loggingTaskIdKey).Select(column => column.Name).ToArray();
        }
    }
}