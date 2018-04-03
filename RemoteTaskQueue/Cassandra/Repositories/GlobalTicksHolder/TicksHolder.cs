using System;
using System.Collections.Concurrent;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    // todo (maybe-optimize): по-хорошему надо распилить на две CF
    // здесь реализуется не очень хороший паттерн - в одной CF метки времени для записи выбираются разными способами: ticks и long.MaxValue - ticks
    public class TicksHolder : ITicksHolder
    {
        public TicksHolder(ICassandraCluster cassandraCluster, ISerializer serializer, IRemoteTaskQueueSettings settings)
        {
            this.cassandraCluster = cassandraCluster;
            this.serializer = serializer;
            keyspaceName = settings.QueueKeyspace;
        }

        public void UpdateMaxTicks([NotNull] string name, long ticks)
        {
            long maxTicks;
            if(persistedMaxTicks.TryGetValue(name, out maxTicks) && ticks <= maxTicks)
                return;
            RetrieveColumnFamilyConnection().AddColumn(name, new Column
                {
                    Name = maxTicksColumnName,
                    Timestamp = ticks,
                    Value = serializer.Serialize(ticks),
                    TTL = null,
                });
            persistedMaxTicks.AddOrUpdate(name, ticks, (key, oldMaxTicks) => Math.Max(ticks, oldMaxTicks));
        }

        public long GetMaxTicks([NotNull] string name)
        {
            Column column;
            if(!RetrieveColumnFamilyConnection().TryGetColumn(name, maxTicksColumnName, out column))
                return 0;
            return serializer.Deserialize<long>(column.Value);
        }

        public void UpdateMinTicks([NotNull] string name, long ticks)
        {
            long minTicks;
            if(persistedMinTicks.TryGetValue(name, out minTicks) && ticks >= minTicks)
                return;
            RetrieveColumnFamilyConnection().AddColumn(name, new Column
                {
                    Name = minTicksColumnName,
                    Timestamp = long.MaxValue - ticks,
                    Value = serializer.Serialize(long.MaxValue - ticks),
                    TTL = null,
                });
            persistedMinTicks.AddOrUpdate(name, ticks, (key, oldMinTicks) => Math.Min(ticks, oldMinTicks));
        }

        public long GetMinTicks([NotNull] string name)
        {
            Column column;
            if(!RetrieveColumnFamilyConnection().TryGetColumn(name, minTicksColumnName, out column))
                return 0;
            return long.MaxValue - serializer.Deserialize<long>(column.Value);
        }

        [NotNull]
        private IColumnFamilyConnection RetrieveColumnFamilyConnection()
        {
            return cassandraCluster.RetrieveColumnFamilyConnection(keyspaceName, ColumnFamilyName);
        }

        public void ResetInMemoryState()
        {
            persistedMaxTicks.Clear();
            persistedMinTicks.Clear();
        }

        public const string ColumnFamilyName = "ticksHolder";
        private const string maxTicksColumnName = "MaxTicks";
        private const string minTicksColumnName = "MinTicks";
        private readonly ICassandraCluster cassandraCluster;
        private readonly ISerializer serializer;
        private readonly string keyspaceName;
        private readonly ConcurrentDictionary<string, long> persistedMaxTicks = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, long> persistedMinTicks = new ConcurrentDictionary<string, long>();
    }
}