using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;

namespace RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public class ChildTaskIndex : IChildTaskIndex
    {
        public ChildTaskIndex(ICassandraCluster cassandraCluster, IRemoteTaskQueueSettings settings, ISerializer serializer, ITaskMetaStorage taskMetaStorage)
        {
            this.cassandraCluster = cassandraCluster;
            this.settings = settings;
            this.serializer = serializer;
            this.taskMetaStorage = taskMetaStorage;
        }

        public void WriteIndexRecord([NotNull] TaskMetaInformation taskMeta, long timestamp)
        {
            if (string.IsNullOrEmpty(taskMeta.ParentTaskId))
                return;
            var ttl = taskMeta.GetTtl();
            cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, ColumnFamilyName).AddColumn(taskMeta.ParentTaskId, new Column
                {
                    Name = taskMeta.Id,
                    Timestamp = timestamp,
                    Value = serializer.Serialize(taskMeta.Id),
                    TTL = ttl.HasValue ? (int)ttl.Value.TotalSeconds : (int?)null,
                });
        }

        [NotNull]
        public string[] GetChildTaskIds([NotNull] string taskId)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, ColumnFamilyName);
            var indexedChildren = connection.GetRow(taskId).Select(column => serializer.Deserialize<string>(column.Value)).ToArray();
            return taskMetaStorage.Read(indexedChildren).Keys.ToArray();
        }

        public const string ColumnFamilyName = "childTaskIndex";
        private readonly ICassandraCluster cassandraCluster;
        private readonly IRemoteTaskQueueSettings settings;
        private readonly ISerializer serializer;
        private readonly ITaskMetaStorage taskMetaStorage;
    }
}