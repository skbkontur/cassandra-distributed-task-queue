using System.Linq;
using System.Reflection;

using ExchangeService.UserClasses;

using RemoteQueue.Configuration;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

using TestCommon;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    [WithTestRemoteTaskQueue]
    public class WithColumnFamilies : EdiTestSuiteWrapperAttribute
    {
        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            var columnFamilies = suiteContext.Container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily
                        {
                            Name = TestCounterRepository.CfName,
                        },
                    new ColumnFamily
                        {
                            Name = CassandraTestTaskLogger.columnFamilyName
                        }
                }).ToArray();
            suiteContext.Container.DropAndCreateDatabase(columnFamilies);
        }
    }
}