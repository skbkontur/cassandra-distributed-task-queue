using System;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace RemoteTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchClient : ElasticsearchClient, IRtqElasticsearchClient
    {
        public RtqElasticsearchClient(params Uri[] uris)
            : base(uris)
        {
        }
    }
}