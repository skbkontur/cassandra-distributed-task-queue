using System;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;

namespace RemoteQueue.Handling
{
    internal delegate void OnIndexMeta(Tuple<string, ColumnInfo> taskInfo, TaskMetaInformation meta);
}