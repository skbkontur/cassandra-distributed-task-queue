using GroBuf;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteQueue.Cassandra.Primitives
{
    public class BlobStorage<T> : ColumnFamilyRepositoryBase, IBlobStorage<T>
    {
        public BlobStorage(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime, string columnFamilyName)
            : base(parameters, columnFamilyName)
        {
            this.serializer = serializer;
            this.globalTime = globalTime;
        }

        public void Write(string id, T element)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(id, new Column
                {
                    Name = "Data",
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(element)
                });
        }

        public T Read(string id)
        {
            IColumnFamilyConnection connection = RetrieveColumnFamilyConnection();
            Column column;
            if(connection.TryGetColumn(id, "Data", out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
    }
}