using System.Linq;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.FunctionalTests
{
    public class AndResetCassandraState : GroboTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            Log.For(this).Info("Resetting test RTQ cassandra state");
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