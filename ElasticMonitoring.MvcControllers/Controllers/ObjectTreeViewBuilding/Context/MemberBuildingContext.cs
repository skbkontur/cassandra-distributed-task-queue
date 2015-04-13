using System;
using System.Linq;

using SKBKontur.Catalogue.Objects;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Controllers.ObjectTreeViewBuilding.Context
{
    internal class MemberBuildingContext
    {
        public Type DeclaredType { get; set; }
        public string[] PathFromRoot { get; set; }

        public string Last()
        {
            return PathFromRoot.LastOrDefault().Return(x => x, "");
        }
    }
}