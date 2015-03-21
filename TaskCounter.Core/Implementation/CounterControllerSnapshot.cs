namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public class CounterControllerSnapshot
    {
        public long CountollerTicks { get; set; }
        public CompositeCounterSnapshot CounterSnapshot { get; set; }
    }
}