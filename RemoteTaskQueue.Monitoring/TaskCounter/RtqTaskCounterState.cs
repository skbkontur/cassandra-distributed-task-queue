using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.TaskCounter
{
    public class RtqTaskCounterState
    {
        public RtqTaskCounterState()
        {
            LastBladeOffset = null;
            TaskMetas = new Dictionary<string, RtqTaskCounterStateTaskMeta>();
        }

        [CanBeNull]
        public string LastBladeOffset { get; set; }

        [NotNull]
        public Dictionary<string, RtqTaskCounterStateTaskMeta> TaskMetas { get; set; }

        public override string ToString()
        {
            return $"LastBladeOffset: {LastBladeOffset}";
        }
    }
}