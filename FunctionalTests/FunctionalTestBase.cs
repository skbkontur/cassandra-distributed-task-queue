using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Handling;

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

        protected TaskTopicAndState TaskTopicAndState(string taskName, TaskState taskState)
        {
            return new TaskTopicAndState(taskTopicResolver.GetTaskTopic(taskName), taskState);
        }

        private ITaskTopicResolver taskTopicResolver;
    }
}