namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class CounterControllerSnapshot
    {
        public long ControllerTicks { get; set; }
        public CompositeCounterSnapshot CounterSnapshot { get; set; }
    }
}