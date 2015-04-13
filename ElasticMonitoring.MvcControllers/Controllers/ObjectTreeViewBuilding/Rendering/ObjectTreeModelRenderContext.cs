namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Rendering
{
    public class ObjectTreeModelRenderContext
    {
        public ObjectTreeModelRenderContext(int index = 0)
        {
            GlobalIndex = index;
        }

        public int GlobalIndex { get; private set; }

        public void IncrementGlobalIndex()
        {
            GlobalIndex++;
        }
    }
}