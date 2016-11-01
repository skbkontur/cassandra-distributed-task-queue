using System;
using System.Diagnostics;
using System.Threading;

using Elasticsearch.Net;

using log4net;

using SKBKontur.Catalogue.Core.Configuration.Settings;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class ElasticAvailabilityChecker
    {
        public ElasticAvailabilityChecker(InternalDataElasticsearchFactory elasticsearchClientFactory, IApplicationSettings applicationSettings)
        {
            elasticsearchClient = elasticsearchClientFactory.GetClient();
            if(!applicationSettings.TryGetTimeSpan("ElasticAliveCheckTimeout", out timeout))
                timeout = TimeSpan.FromSeconds(20);
            logger.LogInfoFormat("ElasticAliveCheckTimeout is {0}", timeout);
        }

        public void WaitAlive()
        {
            logger.LogInfoFormat("Checking Elasticsearch is alive");
            bool isOk = false;
            var w = Stopwatch.StartNew();
            do
            {
                if(!IsAlive())
                    logger.LogInfoFormat("ES is dead.");
                else
                {
                    isOk = true;
                    break;
                } 
                Thread.Sleep(1000);
            } while(w.Elapsed < timeout);
            if(isOk)
                logger.LogInfoFormat("Checking OK");
            else
            {
                logger.LogWarnFormat("Checking FAIL. Exiting");
                throw new InvalidOperationException("Elasticsearch is dead - cannot start");
            }
        }

        private bool IsAlive()
        {
            try
            {
                var response = elasticsearchClient.Info();
                if (!response.Success)
                    return false;
                return (int)response.Response["status"] == 200;
            }
            catch(Exception e)
            {
                logger.LogWarnFormat("CRASH: {0}", e.ToString());
                return false;
            }
        }

        private readonly IElasticsearchClient elasticsearchClient;
        private static readonly ILog logger = LogManager.GetLogger("ElasticAvailabilityChecker");
        private readonly TimeSpan timeout;
    }
}