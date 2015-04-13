namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding
{
    public class ObjectTreeModelValue
    {
        public ObjectTreeModelValue()
        {
            IsHtml = false;   
        }
         
        public string Value { get; set; }
        public bool IsHtml { get; set; }
    }
}