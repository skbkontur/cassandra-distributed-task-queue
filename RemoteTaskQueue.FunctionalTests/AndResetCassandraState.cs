using System.Linq;

using GroboContainer.Core;

using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery.Impl.TestContext;

namespace RemoteTaskQueue.FunctionalTests
{
    public class AndResetCassandraState : EdiTestMethodWrapperAttribute
    {
        public override sealed void SetUp(string testName, IEditableEdiTestContext suiteContext, IEditableEdiTestContext methodContext)
        {
            ResetCassandraState(suiteContext.Container);
        }

        public static void ResetCassandraState(IContainer container)
        {
            var columnFamilies = container.Get<IColumnFamilyRegistry>().GetAllColumnFamilyNames().Concat(new[]
                {
                    new ColumnFamily {Name = ColumnFamilies.TestTaskLoggerCfName},
                    new ColumnFamily {Name = ColumnFamilies.TestCounterRepositoryCfName}
                }).ToArray();
            container.ResetCassandraState(TestRemoteTaskQueueSettings.QueueKeyspaceName, columnFamilies);
        }
    }
}