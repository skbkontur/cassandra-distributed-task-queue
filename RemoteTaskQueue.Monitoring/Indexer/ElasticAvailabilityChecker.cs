using System;
using System.Diagnostics;
using System.Threading;

using RemoteTaskQueue.Monitoring.Storage;
using RemoteTaskQueue.Monitoring.Storage.Utils;

using SKBKontur.Catalogue.Objects;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class ElasticAvailabilityChecker
    {
        public ElasticAvailabilityChecker(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        public void WaitAlive()
        {
            Log.For(this).LogInfoFormat("Checking Elasticsearch is alive");
            var isOk = false;
            var w = Stopwatch.StartNew();
            do
            {
                if (!IsAlive())
                    Log.For(this).LogInfoFormat("ES is dead.");
                else
                {
                    isOk = true;
                    break;
                }
                Thread.Sleep(1000);
            } while (w.Elapsed < timeout);
            if (isOk)
                Log.For(this).LogInfoFormat("Checking OK");
            else
            {
                Log.For(this).LogWarnFormat("Checking FAIL. Exiting");
                throw new InvalidProgramStateException("Elasticsearch is dead - cannot start");
            }
        }

        private bool IsAlive()
        {
            try
            {
                var response = elasticsearchClientFactory.DefaultClient.Value.Info();
                if (!response.Success)
                    return false;
                var legacyStatus = response.Response["status"];
                if (legacyStatus != null)
                    return (int)legacyStatus == 200;
                return response.HttpStatusCode == 200;
            }
            catch (Exception e)
            {
                Log.For(this).LogWarnFormat("CRASH: {0}", e.ToString());
                return false;
            }
        }

        private readonly TimeSpan timeout = TimeSpan.FromSeconds(20);
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}