using System.Linq;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
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
            var columnFamilies = new RtqColumnFamilyRegistry().GetAllColumnFamilyNamesExceptLocks().Concat(new[]
                {
                    new ColumnFamily {Name = GlobalTimeProxy.ColumnFamilyName},
                    new ColumnFamily {Name = RtqColumnFamilyRegistry.LocksColumnFamilyName},
                    new ColumnFamily {Name = TestTaskLogger.ColumnFamilyName},
                    new ColumnFamily {Name = TestCounterRepository.ColumnFamilyName}
                }).ToArray();
            suiteContext.Container.ResetCassandraState(TestRtqSettings.QueueKeyspaceName, columnFamilies);
            suiteContext.Container.Get<IGlobalTime>().ResetInMemoryState();
            suiteContext.Container.Get<IMinTicksHolder>().ResetInMemoryState();
        }
    }
}