using System;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders
{
    internal class SubObject
    {
        public string Name { get; set; }
        public Type DeclaredType { get; set; }
        public object Value { get; set; }
    }
}