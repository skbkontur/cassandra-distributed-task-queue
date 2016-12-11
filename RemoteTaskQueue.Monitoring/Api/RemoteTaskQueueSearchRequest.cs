using System;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.InternalApi.Core;
using SKBKontur.Catalogue.Ranges;

namespace RemoteTaskQueue.Monitoring.Api
{
    [InternalAPI]
    public class RemoteTaskQueueSearchRequest
    {
        public int From { get; set; }
        public int Size { get; set; }

        [NotNull]
        public Range<DateTime> EnqueueDateTimeRange { get; set; }

        [CanBeNull]
        public string QueryString { get; set; }

        [CanBeNull]
        public TaskState[] States { get; set; }

        [CanBeNull]
        public string[] Names { get; set; }
    }
}