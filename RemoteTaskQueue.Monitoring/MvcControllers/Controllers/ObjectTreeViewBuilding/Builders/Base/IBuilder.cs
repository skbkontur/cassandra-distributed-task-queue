using RemoteTaskQueue.Monitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using RemoteTaskQueue.Monitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace RemoteTaskQueue.Monitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Base
{
    internal interface IBuilder
    {
        IBuildingResult Build(object targetObject, BuildingContext buildingContext);
    }
}