using System;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities
{
    public class MonitoringTaskMetadata : IBusinessObject
    {/*
        public MonitoringTaskMetadata(
            string name,
            string id,
            long ticks,
            long minimalStartTicks,
            long? startExecutingTicks,
            TaskState taskState,
            int attempts,
            string parentTaskId)
        {
            Name = name;
            Id = id;
            Ticks = ticks;
            MinimalStartTicks = minimalStartTicks;
            StartExecutingTicks = startExecutingTicks;
            State = taskState;
            Attempts = attempts;
            ParentTaskId = parentTaskId;
        }
        */

        public string Name { get; set; }
        public string TaskId { get; set; }
        public DateTime Ticks { get; set; }
        public DateTime MinimalStartTicks { get; set; }
        public DateTime? StartExecutingTicks { get; set; }
        public TaskState State { get; set; }
        public int Attempts { get; set; }
        public string ParentTaskId { get; set; }
        public string Id { get { return TaskId; } set { } }

        public string ScopeId { get { return TaskId; } set { } }

        public DateTime? LastModificationDateTime { get; set; }
    }
}