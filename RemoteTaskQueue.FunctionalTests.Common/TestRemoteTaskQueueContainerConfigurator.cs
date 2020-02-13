using GroboContainer.Core;

using SKBKontur.Catalogue.TestCore.NUnit.Extensions;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    public static class TestRemoteTaskQueueContainerConfigurator
    {
        public static void ConfigureForTestRemoteTaskQueue(this IContainer container)
        {
            WithCassandra.SetUpCassandra(container, TestRtqSettings.QueueKeyspaceName);
            WithTestRemoteTaskQueue.ConfigureContainer(container);
        }
    }
}