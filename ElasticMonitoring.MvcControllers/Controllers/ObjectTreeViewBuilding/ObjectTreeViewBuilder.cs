using System;
using System.Linq;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Builders.Providers;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Results;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding
{
    internal class ObjectTreeViewBuilder
    {
        public ObjectTreeViewBuilder(IBuildersProvider buildersProvider, Func<object, string[], string> getHyperlinkByPath)
        {
            this.buildersProvider = buildersProvider;
            this.getHyperlinkByPath = getHyperlinkByPath;
        }

        public ObjectTreeModel Build(object value, object objectBuildingContext)
        {
            return Build(value, new BuildingContext
                {
                    RootObject = value,
                    ObjectBuildingContext = objectBuildingContext,
                    MemberBuildingContext = new MemberBuildingContext
                        {
                            DeclaredType = value.GetType(),
                            PathFromRoot = new string[0]
                        }
                });
            ;
        }

        private ObjectTreeModel Build(object value, BuildingContext context)
        {
            foreach(var builder in buildersProvider.GetBuilders())
            {
                var buildingResult = builder.Build(value, context);
                if(buildingResult == NoResult.Instance)
                    continue;

                if(buildingResult is SubObjectsResults)
                {
                    var subs = buildingResult as SubObjectsResults;
                    var result = new ObjectTreeModel
                        {
                            Id = string.Join("_", context.MemberBuildingContext.PathFromRoot),
                            Name = context.MemberBuildingContext.Last(),
                            Link = getHyperlinkByPath(context.RootObject, context.MemberBuildingContext.PathFromRoot)
                        };
                    foreach(var subObject in subs.SubObjects)
                    {
                        var objectTreeModel = Build(subObject.Value, new BuildingContext
                            {
                                RootObject = context.RootObject,
                                MemberBuildingContext = new MemberBuildingContext
                                    {
                                        DeclaredType = subObject.DeclaredType,
                                        PathFromRoot = context.MemberBuildingContext.PathFromRoot.Concat(new[] {subObject.Name}).ToArray()
                                    },
                                ObjectBuildingContext = context.ObjectBuildingContext
                            });
                        if(objectTreeModel != null)
                            result.AddChild(objectTreeModel);
                    }
                    return result;
                }
                if(buildingResult is BuildingResult)
                {
                    return new ObjectTreeModel
                        {
                            Id = string.Join("_", context.MemberBuildingContext.PathFromRoot),
                            Name = context.MemberBuildingContext.Last(),
                            Link = getHyperlinkByPath(context.RootObject, context.MemberBuildingContext.PathFromRoot),
                            Value = buildingResult.Result
                        };
                }
                return null;
            }
            return null;
        }

        private readonly IBuildersProvider buildersProvider;
        private readonly Func<object, string[], string> getHyperlinkByPath;
    }
}