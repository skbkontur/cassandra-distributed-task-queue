using System.Linq;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Primitives;
using SkbKontur.Cassandra.DistributedTaskQueue.Settings;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl
{
    public class TestTaskLogger : ColumnFamilyRepositoryBase, ITestTaskLogger
    {
        public TestTaskLogger(ICassandraCluster cassandraCluster, IRtqSettings rtqSettings)
            :
            base(cassandraCluster, rtqSettings, ColumnFamilies.TestTaskLoggerCfName)
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