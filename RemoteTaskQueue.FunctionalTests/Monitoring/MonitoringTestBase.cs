using System.Diagnostics.CodeAnalysis;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using RemoteTaskQueue.FunctionalTests.Common;
using RemoteTaskQueue.Monitoring.Storage.Client;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.FunctionalTests.Monitoring
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [GroboTestSuite("MonitoringTests"), WithTestRemoteTaskQueue, AndResetMonitoringServiceState]
    public abstract class MonitoringTestBase
    {
        protected void CheckSearch(string q, Timestamp from, Timestamp to, params string[] expectedIds)
        {
            var actualIds = Search(taskSearchClient, q, from, to);
            CollectionAssert.AreEquivalent(expectedIds, actualIds, "q=" + q);
        }

        private static string[] Search(TaskSearchClient taskSearchClient, string q, Timestamp from, Timestamp to)
        {
            var taskSearchResponse = taskSearchClient.Search(new TaskSearchRequest
                {
                    FromTicksUtc = from.Ticks,
                    ToTicksUtc = to.Ticks,
                    QueryString = q,
                }, 0, 1000);
            return taskSearchResponse.Ids;
        }

        [Injected]
        protected readonly TaskSearchClient taskSearchClient;

        [Injected]
        protected readonly MonitoringServiceClient monitoringServiceClient;

        [Injected]
        protected readonly RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue;
    }
}