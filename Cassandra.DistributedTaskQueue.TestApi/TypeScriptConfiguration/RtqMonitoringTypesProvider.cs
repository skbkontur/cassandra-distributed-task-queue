using SkbKontur.Cassandra.DistributedTaskQueue.TestApi.Controllers;
using SkbKontur.TypeScript.ContractGenerator;
using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.Internals;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi.TypeScriptConfiguration
{
    public class RtqMonitoringTypesProvider : IRootTypesProvider
    {
        public ITypeInfo[] GetRootTypes()
        {
            return new[] {TypeInfo.From<RtqMonitoringApiController>()};
        }
    }
}