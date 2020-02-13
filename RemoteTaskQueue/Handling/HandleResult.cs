using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    public class HandleResult
    {
        public FinishAction FinishAction { get; set; }

        public TimeSpan RerunDelay { get; set; }

        [CanBeNull]
        public Exception Error { get; set; }
    }
}