using System.Reflection;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace TestCommon.NUnitWrappers
{
    [WithDefaultSerializer]
    [WithCassandra("CatalogueCluster", "QueueKeyspace")]
    public class WithRemoteLock : EdiTestSuiteWrapperAttribute
    {
        public WithRemoteLock(string remoteLockColumnFamly = null)
        {
            this.remoteLockColumnFamly = remoteLockColumnFamly;
        }

        public override sealed void SetUp(string suiteName, Assembly testAssembly, IEditableEdiTestContext suiteContext)
        {
            suiteContext.Container.ConfigureLockRepository(remoteLockColumnFamly);
        }

        private readonly string remoteLockColumnFamly;
    }
}