using System.Collections.Generic;

using Elasticsearch.Net;

using log4net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSearchIndexSchema
    {
        public TaskSearchIndexSchema(
            IElasticsearchClientFactory elasticsearchClientFactory,
            TaskSearchDynamicSettings dynamicSettings)
        {
            this.dynamicSettings = dynamicSettings;
            elasticsearchClient = elasticsearchClientFactory.GetClient();
        }


        public void DeleteAll()
        {
            elasticsearchClient.IndicesDelete(AllIndexWildcard);
            elasticsearchClient.IndicesDeleteTemplateForAll(IndexTemplateName);
        }

        public const string LastUpdateTicksIndex = "lastupdate-monitoringsearch";
        public const string LastUpdateTicksType = "LastUpdateTicks";
        public const string IndexPrefix = "monitoringsearch-";
        public const string AllIndexWildcard = IndexPrefix + "*";
        public const string IndexTemplateName = "monitoringsearch-template";

        public void ActualizeTemplate()
        {
            var response = elasticsearchClient.IndicesGetTemplateForAll(IndexTemplateName).ProcessResponse(200, 404);
            logger.InfoFormat("TaskSearchIndexSchema: got response {0}", response.HttpStatusCode);
            if(response.HttpStatusCode == 404)
            {
                elasticsearchClient.
                    IndicesCreate(LastUpdateTicksIndex, new
                        {
                            settings = new
                                {
                                    number_of_shards = dynamicSettings.NumberOfShards,
                                    number_of_replicas = dynamicSettings.ReplicaCount,
                                },
                            mappings = new
                                {
                                    LastUpdateTicks = new
                                        {
                                             _all = new {enabled = false},
                                            properties = new
                                                {
                                                    Ticks = new
                                                        {
                                                            type = "long",
                                                            index = "no"
                                                        }
                                                }
                                        }
                                }
                        }).ProcessResponse();

                elasticsearchClient
                    .IndicesPutTemplateForAll(IndexTemplateName, new
                        {
                            template = AllIndexWildcard,
                            settings = new
                                {
                                    number_of_shards = dynamicSettings.NumberOfShards,
                                    number_of_replicas = dynamicSettings.ReplicaCount,
                                },
                            mappings = new
                                {
                                    _default_ = new
                                        {
                                            _all = new {enabled = true},
                                            dynamic_templates = new object[]
                                                {
                                                    new
                                                        {
                                                            template_strings = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "string",
                                                                    mapping = StringTemplate()
                                                                },
                                                        },
                                                    new
                                                        {
                                                            template_dates = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "date",
                                                                    mapping = DateTemplate()
                                                                }
                                                        },
                                                    new
                                                        {
                                                            no_store = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    mapping = new
                                                                        {
                                                                            store = "no"
                                                                        }
                                                                },
                                                        },
                                                },
                                            properties = new Dictionary<string, object>
                                                {
                                                    {"Meta.Name", StringTemplate()},
                                                    {"Meta.Id", StringTemplate()},
                                                    {"Meta.State", StringTemplate()},
                                                    {"Meta.ParentTaskId", StringTemplate()},
                                                    {"Meta.TaskGroupLock", StringTemplate()},
                                                    {"Meta.Attempts", new {type = "integer"}},
                                                    {"Meta.EnqueueTime", DateTemplate()},
                                                    {"Meta.MinimalStartTime", DateTemplate()},
                                                    {"Meta.StartExecutingTime", DateTemplate()},
                                                    {"Meta.FinishExecutingTime", DateTemplate()},
                                                    {"Meta.LastModificationTime", DateTemplate()},
                                                }
                                        }
                                },
                            TaskSearchContext = new
                                {
                                    _ttl = new Dictionary<string, object>()
                                        {
                                            {"enabled", true},
                                            {"default", TaskSearchSettings.SearchRequestExpirationTime},
                                        }
                                }
                        }
                    ).ProcessResponse();
                logger.InfoFormat("TaskSearchIndexSchema: schema created");
            }
        }

        private static object DateTemplate()
        {
            return new {type = "date", format = "dateOptionalTime", store = "no"};
        }

        private static object StringTemplate()
        {
            return new {type = "string", store = "no", index = "not_analyzed"};
        }

        private readonly TaskSearchDynamicSettings dynamicSettings;
        private readonly IElasticsearchClient elasticsearchClient;

        private static readonly ILog logger = LogManager.GetLogger("TaskSearchIndexSchema");
    }
}