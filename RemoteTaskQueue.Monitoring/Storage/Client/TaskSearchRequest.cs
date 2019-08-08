using JetBrains.Annotations;

namespace RemoteTaskQueue.Monitoring.Storage.Client
{
    public class TaskSearchRequest
    {
        public long FromTicksUtc { get; set; }
        public long ToTicksUtc { get; set; }

        [NotNull]
        public string QueryString { get; set; }

        [CanBeNull, ItemNotNull]
        public string[] TaskNames { get; set; }

        [CanBeNull]
        public string[] TaskStates { get; set; }
    }
}