using System.Collections.Generic;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Providers
{
    internal class DefaultBuildersProvider : IBuildersProvider
    {
        public virtual IEnumerable<IBuilder> GetBuilders()
        {
            yield return new NullValuesBuilder();
            yield return new PrimitiveTypesBuilder();
            yield return new DateTimesBuilder();
            yield return new ArraysTypesBuilder();
            yield return new NestedClassTypeBuilder();
        }
    }
}