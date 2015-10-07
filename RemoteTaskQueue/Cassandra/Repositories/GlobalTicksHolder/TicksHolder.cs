using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    // todo (maybe-optimize): по-хорошему надо распилить на две CF
    // здесь реализуется не очень хороший паттерн - в одной CF метки времени для записи выбираются разными способами: ticks и long.MaxValue - ticks
    public class TicksHolder : ColumnFamilyRepositoryBase, ITicksHolder
    {
        public TicksHolder(ISerializer serializer, IColumnFamilyRepositoryParameters parameters)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
        }

        public void UpdateMaxTicks([NotNull] string name, long ticks)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(name, new Column
                {
                    Name = maxTicksColumnName,
                    Timestamp = ticks,
                    Value = serializer.Serialize(ticks)
                });
        }

        public long GetMaxTicks([NotNull] string name)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(!connection.TryGetColumn(name, maxTicksColumnName, out column))
                return 0;
            return serializer.Deserialize<long>(column.Value);
        }

        public void UpdateMinTicks([NotNull] string name, long ticks)
        {
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(name, new Column
                {
                    Name = minTicksColumnName,
                    Timestamp = long.MaxValue - ticks,
                    Value = serializer.Serialize(long.MaxValue - ticks)
                });
        }

        public long GetMinTicks([NotNull] string name)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(!connection.TryGetColumn(name, minTicksColumnName, out column))
                return 0;
            return long.MaxValue - serializer.Deserialize<long>(column.Value);
        }

        public const string columnFamilyName = "ticksHolder";
        private const string maxTicksColumnName = "MaxTicks";
        private const string minTicksColumnName = "MinTicks";
        private readonly ISerializer serializer;
    }
}