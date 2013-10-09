using GroBuf;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

using log4net;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public class TicksHolder : ColumnFamilyRepositoryBase, ITicksHolder
    {
        public TicksHolder(ISerializer serializer, IColumnFamilyRepositoryParameters parameters)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
        }

        public long UpdateMaxTicks(string name, long ticks)
        {
            logger.InfoFormat("UpdateMaxTicks(name {0}, ticks {1})", name, ticks);
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(name, new Column
                {
                    Name = maxTicksColumnName,
                    Timestamp = ticks,
                    Value = serializer.Serialize(ticks)
                });
            return GetMaxTicks(name);
        }

        public long GetMaxTicks(string name)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(!connection.TryGetColumn(name, maxTicksColumnName, out column))
                return 0;
            return serializer.Deserialize<long>(column.Value);
        }

        public long UpdateMinTicks(string name, long ticks)
        {
            logger.InfoFormat("UpdateMinTicks(name {0}, ticks {1})", name, ticks);
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(name, new Column
                {
                    Name = minTicksColumnName,
                    Timestamp = long.MaxValue - ticks,
                    Value = serializer.Serialize(long.MaxValue - ticks)
                });
            return GetMinTicks(name);
        }

        public long GetMinTicks(string name)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(!connection.TryGetColumn(name, minTicksColumnName, out column))
                return 0;
            return long.MaxValue - serializer.Deserialize<long>(column.Value);
        }

        public const string columnFamilyName = "ticksHolder";
        private readonly ISerializer serializer;
        private const string maxTicksColumnName = "MaxTicks";
        private const string minTicksColumnName = "MinTicks";

        private static readonly ILog logger = LogManager.GetLogger(typeof(TicksHolder));
    }
}