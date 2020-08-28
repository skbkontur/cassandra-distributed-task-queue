using System.Diagnostics.CodeAnalysis;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Client;
using SkbKontur.Cassandra.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.Monitoring
{
    [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
    [GroboTestSuite("MonitoringTests"), WithTestRemoteTaskQueue, WithRtqElasticsearchClient, AndResetMonitoringServiceState]
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
        protected readonly SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue remoteTaskQueue;
    }
}