using System;
using System.Diagnostics;
using System.Threading;

using RemoteTaskQueue.Monitoring.Storage;

using SKBKontur.Catalogue.Objects;

using Vostok.Logging.Abstractions;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class ElasticAvailabilityChecker
    {
        public ElasticAvailabilityChecker(RtqElasticsearchClientFactory elasticsearchClientFactory, ILog logger)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
            this.logger = logger.ForContext("CassandraDistributedTaskQueue.ElasticAvailabilityChecker");
        }

        public void WaitAlive()
        {
            logger.Info("Checking Elasticsearch is alive");
            var isOk = false;
            var w = Stopwatch.StartNew();
            do
            {
                if (!IsAlive())
                    logger.Info("ES is dead.");
                else
                {
                    isOk = true;
                    break;
                }
                Thread.Sleep(1000);
            } while (w.Elapsed < timeout);
            if (isOk)
                logger.Info("Checking OK");
            else
            {
                logger.Warn("Checking FAIL. Exiting");
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
                logger.Warn("CRASH: {0}", e.ToString());
                return false;
            }
        }

        private readonly TimeSpan timeout = TimeSpan.FromSeconds(20);
        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
        private readonly ILog logger;
    }
}