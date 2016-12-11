using System.Linq;
using System.Reflection;

using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    [WithTestRemoteTaskQueue]
    public class WithColumnFamilies : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            var columnFamilies = suiteContext.Container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily {Name = ColumnFamilies.TestTaskLoggerCfName},
                    new ColumnFamily {Name = ColumnFamilies.TestCounterRepositoryCfName}
                }).ToArray();
            suiteContext.Container.DropAndCreateDatabase(columnFamilies);
        }
    }
}