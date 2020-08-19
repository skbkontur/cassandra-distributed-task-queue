using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public class DateTimeRange
    {
        public DateTime LowerBound { get; set; }
        public DateTime UpperBound { get; set; }
    }
}