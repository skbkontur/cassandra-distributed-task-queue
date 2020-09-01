using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.BlobStorages;
using SkbKontur.Cassandra.DistributedTaskQueue.Handling;
using SkbKontur.Cassandra.ThriftClient.Abstractions;
using SkbKontur.Cassandra.ThriftClient.Clusters;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public class ChildTaskIndex : IChildTaskIndex
    {
        public ChildTaskIndex(ICassandraCluster cassandraCluster, IRtqSettings rtqSettings, ISerializer serializer, ITaskMetaStorage taskMetaStorage)
        {
            this.cassandraCluster = cassandraCluster;
            this.rtqSettings = rtqSettings;
            this.serializer = serializer;
            this.taskMetaStorage = taskMetaStorage;
        }

        public void WriteIndexRecord([NotNull] TaskMetaInformation taskMeta, long timestamp)
        {
            if (string.IsNullOrEmpty(taskMeta.ParentTaskId))
                return;
            var ttl = taskMeta.GetTtl();
            cassandraCluster.RetrieveColumnFamilyConnection(rtqSettings.QueueKeyspace, ColumnFamilyName).AddColumn(taskMeta.ParentTaskId, new Column
                {
                    Name = taskMeta.Id,
                    Timestamp = timestamp,
                    Value = serializer.Serialize(taskMeta.Id),
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        [NotNull, ItemNotNull]
        public string[] GetChildTaskIds([NotNull] string taskId)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(rtqSettings.QueueKeyspace, ColumnFamilyName);
            var indexedChildren = connection.GetRow(taskId).Select(column => serializer.Deserialize<string>(column.Value)).ToArray();
            return taskMetaStorage.Read(indexedChildren).Keys.ToArray();
        }

        public const string ColumnFamilyName = "childTaskIndex";
        private readonly ICassandraCluster cassandraCluster;
        private readonly IRtqSettings rtqSettings;
        private readonly ISerializer serializer;
        private readonly ITaskMetaStorage taskMetaStorage;
    }
}