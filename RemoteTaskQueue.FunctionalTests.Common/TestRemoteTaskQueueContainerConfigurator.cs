using GroboContainer.Core;

using SKBKontur.Catalogue.NUnit.Extensions.CommonWrappers;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public static class TestRemoteTaskQueueContainerConfigurator
    {
        public static void ConfigureForTestRemoteTaskQueue(this IContainer container)
        {
            WithCassandra.SetUpCassandra(container, TestRemoteTaskQueueSettings.QueueKeyspaceName);
            WithTestRemoteTaskQueue.ConfigureContainer(container);
        }
    }
}