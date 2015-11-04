using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.RemoteTaskQueue.Common;

namespace FunctionalTests
{
    public abstract class FunctionalTestBase : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            var exchangeServiceClient = Container.Get<IExchangeServiceClient>();
            exchangeServiceClient.Start();
            taskTopicResolver = Container.Get<ITaskTopicResolver>();
        }

        public override void TearDown()
        {
            var exchangeServiceClient = Container.Get<IExchangeServiceClient>();
            exchangeServiceClient.Stop();
            base.TearDown();
        }

        protected TaskIndexShardKey TaskIndexShardKey(string taskName, TaskState taskState)
        {
            return new TaskIndexShardKey(taskTopicResolver.GetTaskTopic(taskName), taskState);
        }

        private ITaskTopicResolver taskTopicResolver;
    }
}