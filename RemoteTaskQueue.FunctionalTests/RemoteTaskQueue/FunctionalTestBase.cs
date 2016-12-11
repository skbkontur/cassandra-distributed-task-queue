using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue
{
    public abstract class FunctionalTestBase : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            var exchangeServiceClient = Container.Get<ExchangeServiceClient>();
            exchangeServiceClient.Start();
            exchangeServiceClient.ChangeTaskTtl(RemoteQueueTestsCassandraSettings.StandardTestTaskTtl);
            taskDataRegistry = Container.Get<ITaskDataRegistry>();
        }

        public override void TearDown()
        {
            Container.Get<ExchangeServiceClient>().Stop();
            base.TearDown();
        }

        protected TaskIndexShardKey TaskIndexShardKey(string taskName, TaskState taskState)
        {
            return new TaskIndexShardKey(taskDataRegistry.GetTaskTopic(taskName), taskState);
        }

        private ITaskDataRegistry taskDataRegistry;
    }
}