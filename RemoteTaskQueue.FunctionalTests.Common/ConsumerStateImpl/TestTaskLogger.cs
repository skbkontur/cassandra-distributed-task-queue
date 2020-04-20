using System.Linq;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Connections;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl
{
    public class TestTaskLogger : ITestTaskLogger
    {
        public TestTaskLogger(ICassandraCluster cassandraCluster, IRtqSettings rtqSettings)
        {
            cfConnection = cassandraCluster.RetrieveColumnFamilyConnection(rtqSettings.QueueKeyspace, ColumnFamilyName);
        }

        public void Log(string loggingTaskIdKey, string taskId)
        {
            cfConnection.AddColumn(loggingTaskIdKey, new Column
                {
                    Name = taskId,
                    Value = new byte[] {1},
                    Timestamp = Timestamp.Now.Ticks
                });
        }

        public string[] GetAll(string loggingTaskIdKey)
        {
            return cfConnection.GetRow(loggingTaskIdKey).Select(column => column.Name).ToArray();
        }

        public const string ColumnFamilyName = "TestTaskLoggerCf";

        private readonly IColumnFamilyConnection cfConnection;
    }
}