using System;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public class ChildTaskIndex : IChildTaskIndex
    {
        public ChildTaskIndex(IColumnFamilyRepositoryParameters repositoryParameters, ISerializer serializer, ITaskMetaStorage taskMetaStorage)
        {
            this.repositoryParameters = repositoryParameters;
            this.serializer = serializer;
            this.taskMetaStorage = taskMetaStorage;
        }

        public void AddMeta([NotNull] TaskMetaInformation meta)
        {
            if(string.IsNullOrEmpty(meta.ParentTaskId))
                return;
            var connection = repositoryParameters.CassandraCluster.RetrieveColumnFamilyConnection(repositoryParameters.Settings.QueueKeyspace, columnFamilyName);
            connection.AddColumn(meta.ParentTaskId, new Column
                {
                    Name = meta.Id,
                    Timestamp = DateTime.UtcNow.Ticks,
                    Value = serializer.Serialize(meta.Id)
                });
        }

        [NotNull]
        public string[] GetChildTaskIds([NotNull] string taskId)
        {
            var connection = repositoryParameters.CassandraCluster.RetrieveColumnFamilyConnection(repositoryParameters.Settings.QueueKeyspace, columnFamilyName);
            var indexedChildren = connection.GetRow(taskId).Select(column => serializer.Deserialize<string>(column.Value)).ToArray();
            return taskMetaStorage.Read(indexedChildren).Keys.ToArray();
        }

        public const string columnFamilyName = "childTaskIndex";

        private readonly IColumnFamilyRepositoryParameters repositoryParameters;
        private readonly ISerializer serializer;
        private readonly ITaskMetaStorage taskMetaStorage;
    }
}