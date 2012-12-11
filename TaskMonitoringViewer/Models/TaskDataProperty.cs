namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models
{
    public class TaskDataProperty
    {
        public string Name { get; set; }
        public ITaskDataValue Value { get; set; }
        public bool Hidden { get; set; }
    }
}