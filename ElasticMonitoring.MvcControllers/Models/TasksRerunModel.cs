namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models
{
    public class TasksRerunModel
    {
        public int Rerunned { get; set; }
        public int NotRerunned { get; set; }
        public string IteratorContext { get; set; }
        public int TotalTasksToRerun { get; set; } 
    }
    
    public class TasksCancelModel
    {
        public int Canceled { get; set; }
        public int NotCanceled { get; set; }
        public string IteratorContext { get; set; }
        public int TotalTasksToCancel { get; set; } 
    }
}