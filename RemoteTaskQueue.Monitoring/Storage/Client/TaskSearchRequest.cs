namespace RemoteTaskQueue.Monitoring.Storage.Client
{
    public class TaskSearchRequest
    {
        public long FromTicksUtc { get; set; }
        public long ToTicksUtc { get; set; }
        public string QueryString { get; set; }
        public string[] TaskNames { get; set; }
        public string[] TaskStates { get; set; }
    }
}