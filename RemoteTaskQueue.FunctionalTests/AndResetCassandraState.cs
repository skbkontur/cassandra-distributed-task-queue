using System.Linq;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests
{
    public class AndResetCassandraState : EdiTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            Log.For(this).Info("Resetting test RTQ cassanda state");
            var columnFamilies = suiteContext.Container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily {Name = ColumnFamilies.TestTaskLoggerCfName},
                    new ColumnFamily {Name = ColumnFamilies.TestCounterRepositoryCfName}
                }).ToArray();
            suiteContext.Container.ResetCassandraState(TestRemoteTaskQueueSettings.QueueKeyspaceName, columnFamilies);
            suiteContext.Container.Get<TicksHolder>().ResetInMemoryState();
        }
    }
}