using System.Collections.Generic;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Providers;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.TaskDetailsTreeView
{
    internal class TaskDataBuildersProvider : DefaultBuildersProvider
    {
        public override IEnumerable<IBuilder> GetBuilders()
        {
            yield return new NullValuesBuilder();
            yield return new PrimitiveTypesBuilderWithSpecialValue();
            yield return new ByteArrayBuilder();
            yield return new TimeGuidBuilder();
            foreach(var builder in base.GetBuilders())
                yield return builder;
        }
    }
}