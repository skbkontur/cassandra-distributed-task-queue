namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.MvcControllers.Models
{
    public class ProcessingTaskCountModel
    {
        public int Count { get; set; }
        public long UpdateTimeJsTicks { get; set; }
        public long StartTimeJsTicks { get; set; }
    }
}