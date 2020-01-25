using System.Linq;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Abstractions;

using SKBKontur.Catalogue.ServiceLib.Logging;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.FunctionalTests
{
    public class AndResetCassandraState : GroboTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableGroboTestContext suiteContext, IEditableGroboTestContext methodContext)
        {
            Log.For(this).Info("Resetting test RTQ cassandra state");
            var columnFamilies = new RtqColumnFamilyRegistry().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily {Name = "GlobalMaxTicks"},
                    new ColumnFamily {Name = ColumnFamilies.TestTaskLoggerCfName},
                    new ColumnFamily {Name = ColumnFamilies.TestCounterRepositoryCfName}
                }).ToArray();
            suiteContext.Container.ResetCassandraState(TestRemoteTaskQueueSettings.QueueKeyspaceName, columnFamilies);
            suiteContext.Container.Get<IGlobalTime>().ResetInMemoryState();
            suiteContext.Container.Get<IMinTicksHolder>().ResetInMemoryState();
        }
    }
}