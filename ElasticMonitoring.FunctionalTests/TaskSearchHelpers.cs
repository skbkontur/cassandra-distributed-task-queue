using System;
using System.Diagnostics;
using System.Threading;

using NUnit.Framework;

using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.FunctionalTests
{
    internal class TaskSearchHelpers
    {
        public static void WaitFor(Func<bool> func, TimeSpan timeout, int checkTimeout = 99)
        {
            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.Elapsed < timeout)
            {
                Thread.Sleep(checkTimeout);
                if(func())
                    return;
            }
            Assert.Fail("Условия ожидания не выполнены за {0}", timeout);
        }

        public static void CheckSearch(ITaskSearchClient taskSearchClient, string q, DateTime from, DateTime to, params string[] expectedIds)
        {
            var actualIds = Search(taskSearchClient, q, from, to);
            CollectionAssert.AreEquivalent(expectedIds, actualIds, "q=" + q);
        }

        private static string[] Search(ITaskSearchClient taskSearchClient, string q, DateTime from, DateTime to)
        {
            var taskSearchResponse = taskSearchClient.SearchFirst(new TaskSearchRequest
                {
                    FromTicksUtc = from.ToUniversalTime().Ticks,
                    ToTicksUtc = to.ToUniversalTime().Ticks,
                    QueryString = q
                }, 0, 1000);
            return taskSearchResponse.Ids;
        }
    }
}