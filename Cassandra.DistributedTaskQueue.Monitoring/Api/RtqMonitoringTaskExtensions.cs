using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api
{
    public static class RtqMonitoringTaskExtensions
    {
        public static RtqMonitoringTaskMeta ToMonitoringTaskMeta(this TaskMetaInformation meta)
        {
            return new RtqMonitoringTaskMeta
                {
                    Name = meta.Name,
                    Id = meta.Id,
                    Ticks = meta.Ticks,
                    MinimalStartTicks = meta.MinimalStartTicks,
                    StartExecutingTicks = meta.StartExecutingTicks,
                    FinishExecutingTicks = meta.FinishExecutingTicks,
                    LastModificationTicks = meta.LastModificationTicks,
                    ExpirationTimestampTicks = meta.ExpirationTimestampTicks,
                    ExpirationModificationTicks = meta.ExpirationModificationTicks,
                    ExecutionDurationTicks = meta.ExecutionDurationTicks,
                    State = meta.State,
                    Attempts = meta.Attempts,
                    ParentTaskId = meta.ParentTaskId,
                };
        }
    }
}