using System;

using GroBuf;

using SkbKontur.Cassandra.DistributedLock;
using SkbKontur.Cassandra.DistributedLock.RemoteLocker;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;
using SkbKontur.Cassandra.ThriftClient.Connections;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl
{
    public class TestCounterRepository : ITestCounterRepository
    {
        public TestCounterRepository(ICassandraCluster cassandraCluster,
                                     ISerializer serializer,
                                     IGlobalTime globalTime,
                                     IRtqSettings rtqSettings)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
            var keyspaceName = rtqSettings.NewQueueKeyspace;
            cfConnection = cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, ColumnFamilyName);
            var remoteLockImplementationSettings = CassandraRemoteLockImplementationSettings.Default(keyspaceName, RtqColumnFamilyRegistry.LocksColumnFamilyName);
            var remoteLockImplementation = new CassandraRemoteLockImplementation(cassandraCluster, serializer, remoteLockImplementationSettings);
            remoteLockCreator = new RemoteLocker(remoteLockImplementation, new RemoteLockerMetrics(keyspaceName), Log.DefaultLogger);
        }

        public int GetCounter(string taskId)
        {
            using (Lock(taskId))
                return GetCounterInternal(taskId);
        }

        public int IncrementCounter(string taskId)
        {
            using (Lock(taskId))
            {
                var cnt = GetCounterInternal(taskId);
                SetCounterInternal(taskId, cnt + 1);
                return cnt + 1;
            }
        }

        public int DecrementCounter(string taskId)
        {
            using (Lock(taskId))
            {
                var cnt = GetCounterInternal(taskId);
                SetCounterInternal(taskId, cnt - 1);
                return cnt - 1;
            }
        }

        public void SetValueForCounter(string taskId, int value)
        {
            using (Lock(taskId))
                SetCounterInternal(taskId, value);
        }

        private int GetCounterInternal(string taskId)
        {
            if (cfConnection.TryGetColumn(taskId, dataColumnName, out var column))
                return serializer.Deserialize<int>(column.Value);
            return 0;
        }

        private void SetCounterInternal(string taskId, int value)
        {
            cfConnection.AddColumn(taskId, new Column
                {
                    Name = dataColumnName,
                    Timestamp = globalTime.UpdateNowTimestamp().Ticks,
                    Value = serializer.Serialize(value),
                    TTL = (int)TimeSpan.FromHours(1).TotalSeconds,
                });
        }

        private IRemoteLock Lock(string taskId)
        {
            return remoteLockCreator.Lock($"TestCounterRepository_{taskId}");
        }

        public const string ColumnFamilyName = "TestCounterRepositoryCf";

        private const string dataColumnName = "X";
        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
        private readonly IRemoteLockCreator remoteLockCreator;
        private readonly IColumnFamilyConnection cfConnection;
    }
}