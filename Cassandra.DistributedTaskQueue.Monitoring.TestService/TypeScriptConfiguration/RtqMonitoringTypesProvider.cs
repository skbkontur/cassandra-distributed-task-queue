using SkbKontur.TypeScript.ContractGenerator;
using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.Internals;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService.TypeScriptConfiguration
{
    public class RtqMonitoringTypesProvider : IRootTypesProvider
    {
        public ITypeInfo[] GetRootTypes()
        {
            return new[] {TypeInfo.From<RtqMonitoringApiController>()};
        }
    }
}