using System.Text.RegularExpressions;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Api;
using SkbKontur.Cassandra.TimeBasedUuid;
using SkbKontur.TypeScript.ContractGenerator;
using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.CodeDom;
using SkbKontur.TypeScript.ContractGenerator.Internals;
using SkbKontur.TypeScript.ContractGenerator.TypeBuilders;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi.TypeScriptConfiguration
{
    public class RtqMonitoringCustomTypeGenerator : ICustomTypeGenerator
    {
        public string GetTypeLocation(ITypeInfo type)
        {
            if (InternalApiTypeBuildingContext.Accept(type))
                return InternalApiTypeBuildingContext.GetApiName(type);
            return type.IsGenericType ? new Regex("`.*$").Replace(type.Name, "") : type.Name;
        }

        public ITypeBuildingContext ResolveType(string initialUnitPath, ITypeGenerator typeGenerator, ITypeInfo type, ITypeScriptUnitFactory unitFactory)
        {
            if (type.Equals(TypeInfo.From<TimeGuid>()))
                return TypeBuilding.RedirectToType("TimeGuid", @"..\DataTypes\TimeGuid", type);
            if (type.Equals(TypeInfo.From<Timestamp>()))
                return TypeBuilding.RedirectToType("Timestamp", @"..\DataTypes\Timestamp", type);
            if (type.Equals(TypeInfo.From<TimestampRange>()))
                return TypeBuilding.RedirectToType("DateTimeRange", @"..\DataTypes\DateTimeRange", type);
            if (InternalApiTypeBuildingContext.Accept(type))
                return new InternalApiTypeBuildingContext(unitFactory.GetOrCreateTypeUnit(initialUnitPath), type);
            return null;
        }

        public TypeScriptTypeMemberDeclaration ResolveProperty(TypeScriptUnit unit, ITypeGenerator typeGenerator, ITypeInfo type, IPropertyInfo property)
        {
            return null;
        }
    }
}