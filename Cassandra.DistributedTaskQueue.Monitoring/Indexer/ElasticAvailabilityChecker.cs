using System;
using System.Diagnostics;
using System.Threading;

using Elasticsearch.Net;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Indexer
{
    public class ElasticAvailabilityChecker
    {
        public ElasticAvailabilityChecker(IRtqElasticsearchClient elasticClient, ILog logger)
        {
            this.elasticClient = elasticClient;
            this.logger = logger.ForContext("CassandraDistributedTaskQueue").ForContext(nameof(ElasticAvailabilityChecker));
        }

        public void WaitAlive()
        {
            logger.Info("Checking ElasticSearch is alive");
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
                throw new InvalidOperationException("ElasticSearch is dead - cannot start");
            }
        }

        private bool IsAlive()
        {
            try
            {
                var response = elasticClient.Ping<StringResponse>();
                if (!response.Success)
                    return false;
                return response.HttpStatusCode == 200;
            }
            catch (Exception e)
            {
                logger.Warn(e, "CRASH");
                return false;
            }
        }

        private readonly TimeSpan timeout = TimeSpan.FromSeconds(20);
        private readonly IRtqElasticsearchClient elasticClient;
        private readonly ILog logger;
    }
}