using System;
using System.Collections.Generic;
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

        public static void CheckSearch(ITaskSearchClient taskSearchClient, string q, DateTime from, DateTime to, params string[] ids)
        {
            CollectionAssert.AreEquivalent(ids, Search(taskSearchClient, q, @from, to), "q=" + q);
        }

        private static string[] Search(ITaskSearchClient taskSearchClient, string q, DateTime from, DateTime to)
        {
            //todo kill q2 and delete all ES data
            //var q2 = string.Format("({0}) AND (Meta.EnqueueTime:[\"{1}\" TO \"{2}\"])", q, ToIsoTime(@from), ToIsoTime(to));
            var taskSearchResponse = taskSearchClient.SearchFirst(new TaskSearchRequest()
                {
                    FromTicksUtc = @from.ToUniversalTime().Ticks,
                    ToTicksUtc = to.ToUniversalTime().Ticks,
                    QueryString = q
                });
            var result = new List<string>();
            if(taskSearchResponse.NextScrollId != null)
            {
                do
                {
                    foreach(var id in taskSearchResponse.Ids)
                        result.Add(id);
                    taskSearchResponse = taskSearchClient.SearchNext(taskSearchResponse.NextScrollId);
                } while(taskSearchResponse.Ids != null && taskSearchResponse.Ids.Length > 0);
            }
            return result.ToArray();
        }
    }
}