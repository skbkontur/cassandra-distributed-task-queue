using System.Collections.Generic;

using RemoteTaskQueue.Monitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base;

namespace RemoteTaskQueue.Monitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Providers
{
    internal interface IBuildersProvider
    {
        IEnumerable<IBuilder> GetBuilders();
    }
}