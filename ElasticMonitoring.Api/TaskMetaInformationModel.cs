using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Api
{
    [InternalAPI]
    public class TaskMetaInformationModel
    {
        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string Id { get; set; }

        public DateTime EnqueueDateTime { get; set; }
        public DateTime MinimalStartDateTime { get; set; }
        public DateTime? StartExecutingDateTime { get; set; }
        public DateTime? FinishExecutingDateTime { get; set; }
        public DateTime? LastModificationDateTime { get; set; }
        public DateTime? ExpirationTimestamp { get; set; }
        public DateTime? ExpirationModificationDateTime { get; set; }
        public TaskState State { get; set; }
        public int Attempts { get; set; }
        public string ParentTaskId { get; set; }
        public string TaskGroupLock { get; set; }
        public string TraceId { get; set; }
        public bool TraceIsActive { get; set; }
    }
}