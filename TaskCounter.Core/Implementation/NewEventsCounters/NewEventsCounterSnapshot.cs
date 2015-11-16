using System.Collections.Generic;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.NewEventsCounters
{
    public class NewEventsCounterSnapshot
    {
        public string[] Tasks { get; set; }
        public Dictionary<string, long> NotCountedNewTasks { get; set; }
    }
}