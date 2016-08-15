using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Catalogue.Objects;

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

        public void AddMeta([NotNull] TaskMetaInformation meta)
        {
            if(string.IsNullOrEmpty(meta.ParentTaskId))
                return;
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamilyName);
            connection.AddColumn(meta.ParentTaskId, new Column
                {
                    Name = meta.Id,
                    Timestamp = Timestamp.Now.Ticks,
                    Value = serializer.Serialize(meta.Id)
                });
        }

        [NotNull]
        public string[] GetChildTaskIds([NotNull] string taskId)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, columnFamilyName);
            var indexedChildren = connection.GetRow(taskId).Select(column => serializer.Deserialize<string>(column.Value)).ToArray();
            return taskMetaStorage.Read(indexedChildren).Keys.ToArray();
        }

        public const string columnFamilyName = "childTaskIndex";

        private readonly ICassandraCluster cassandraCluster;
        private readonly IRemoteTaskQueueSettings settings;
        private readonly ISerializer serializer;
        private readonly ITaskMetaStorage taskMetaStorage;
    }
}