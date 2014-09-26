using System;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.BlobStorages;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace RemoteQueue.Cassandra.Repositories.Indexes.ChildTaskIndex
{
    public class ChildTaskIndex : IChildTaskIndex
    {
        public ChildTaskIndex(IColumnFamilyRepositoryParameters repositoryParameters, ISerializer serializer, ITaskMetaInformationBlobStorage metaInformationBlobStorage)
        {
            this.repositoryParameters = repositoryParameters;
            this.serializer = serializer;
            this.metaInformationBlobStorage = metaInformationBlobStorage;
        }

        public void AddMeta(TaskMetaInformation meta)
        {
            if (string.IsNullOrEmpty(meta.ParentTaskId))
                return;
            var connection = repositoryParameters.CassandraCluster.RetrieveColumnFamilyConnection(repositoryParameters.Settings.QueueKeyspace, columnFamilyName);
            connection.AddColumn(meta.ParentTaskId, new Column
                {
                    Name = meta.Id,
                    Timestamp = DateTime.UtcNow.Ticks,
                    Value = serializer.Serialize(meta.Id)
                });
        }

        public string[] GetChildTaskIds(string taskId)
        {
            var connection = repositoryParameters.CassandraCluster.RetrieveColumnFamilyConnection(repositoryParameters.Settings.QueueKeyspace, columnFamilyName);
            var indexedChildren = connection.GetRow(taskId).Select(column => serializer.Deserialize<string>(column.Value)).ToArray();
            return metaInformationBlobStorage.Read(indexedChildren).Select(x => x.Id).ToArray();
        }

        private readonly IColumnFamilyRepositoryParameters repositoryParameters;
        private readonly ISerializer serializer;
        private readonly ITaskMetaInformationBlobStorage metaInformationBlobStorage;
        public const string columnFamilyName = "childTaskIndex";
    }
}