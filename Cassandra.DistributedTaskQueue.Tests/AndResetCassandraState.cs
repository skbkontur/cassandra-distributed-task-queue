using System.Linq;

using GroboContainer.NUnitExtensions;
using GroboContainer.NUnitExtensions.Impl.TestContext;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common;
using SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.ConsumerStateImpl;
using SkbKontur.Cassandra.GlobalTimestamp;
using SkbKontur.Cassandra.ThriftClient.Abstractions;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests
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
            suiteContext.Container.ResetCassandraState(columnFamilies);
            suiteContext.Container.Get<IGlobalTime>().ResetInMemoryState();
            suiteContext.Container.Get<IMinTicksHolder>().ResetInMemoryState();
        }
    }
}