using System.Collections.Generic;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

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
            var connection = RetrieveColumnFamilyConnection();
            connection.AddColumn(id, new Column
                {
                    Name = "Data",
                    Timestamp = globalTime.GetNowTicks(),
                    Value = serializer.Serialize(element)
                });
        }

        public T Read(string id)
        {
            var connection = RetrieveColumnFamilyConnection();
            Column column;
            if(connection.TryGetColumn(id, "Data", out column))
                return serializer.Deserialize<T>(column.Value);
            return default(T);
        }

        public IEnumerable<T> ReadAll(int batchSize = 1000)
        {
            var connection = RetrieveColumnFamilyConnection();
            var keys = connection.GetKeys(batchSize);
            return new SeparateOnBatchesEnumerable<string>(keys, batchSize).SelectMany(
                batch => connection.GetRows(batch, "Data", 1)
                                   .Where(x => x.Value.Length > 0)
                                   .Select(x => serializer.Deserialize<T>(x.Value[0].Value)));
        }

        private readonly ISerializer serializer;
        private readonly IGlobalTime globalTime;
    }
}