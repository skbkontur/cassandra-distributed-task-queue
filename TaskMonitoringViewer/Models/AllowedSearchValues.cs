using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class AllowedSearchValues
    {
        public string[] Names { get; set; }
        public TaskState[] States { get; set; }
    }
}