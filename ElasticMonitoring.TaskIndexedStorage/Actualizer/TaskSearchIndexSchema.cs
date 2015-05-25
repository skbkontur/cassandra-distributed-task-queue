using System.Collections.Generic;

using Elasticsearch.Net;

using log4net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSearchIndexSchema
    {
        public TaskSearchIndexSchema(
            IElasticsearchClientFactory elasticsearchClientFactory,
            TaskSchemaDynamicSettings settings)
        {
            this.settings = settings;
            elasticsearchClient = elasticsearchClientFactory.GetClient();
        }

        public void RemoveOldVersionTemplates()
        {
            //NOTE first version template. used in RosAlko
            elasticsearchClient.IndicesDeleteTemplateForAll("monitoringsearch-template").ProcessResponse(200, 404);
        }

        public void ActualizeTemplate()
        {
            PutDataTemplate(settings.TemplateNamePrefix + DataTemplateSuffix, settings.IndexPrefix + "*");
            PutDataTemplate(settings.TemplateNamePrefix + OldDataTemplateSuffix, settings.OldDataIndex);
            CreateIndexIfNotExists(settings.OldDataIndex, new {});
            CreateLastUpdateTicksIndex(settings.LastTicksIndex);
        }

        private void CreateLastUpdateTicksIndex(string indexName)
        {
            CreateIndexIfNotExists(indexName, new
                {
                    settings = new
                        {
                            number_of_shards = settings.NumberOfShards,
                            number_of_replicas = settings.ReplicaCount,
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
                });
        }

        private void CreateIndexIfNotExists(string indexName, object body)
        {
            logger.LogInfoFormat("Attempt to create Index {0}", indexName);

            if(elasticsearchClient.IndicesExists(indexName).ProcessResponse(200, 404).HttpStatusCode == 404)
            {
                logger.LogInfoFormat("Index not exists - creating {0}", indexName);
                elasticsearchClient.IndicesCreate(indexName, body).ProcessResponse();
            }
            else
                logger.LogInfoFormat("Index already exists");
        }

        private void PutDataTemplate(string templateName, string indicesPattern)
        {
            logger.LogInfoFormat("Attempt to put data template name '{0}' pattern '{1}'", templateName, indicesPattern);
            var response = elasticsearchClient.IndicesGetTemplateForAll(templateName).ProcessResponse(200, 404);
            //if(response.HttpStatusCode == 404)
            {
                //logger.LogInfoFormat("Template not exists - creating");
                elasticsearchClient
                    .IndicesPutTemplateForAll(templateName, new
                        {
                            template = indicesPattern,
                            settings = new
                                {
                                    number_of_shards = settings.NumberOfShards,
                                    number_of_replicas = settings.ReplicaCount,
                                },
                            mappings = new
                                {
                                    _default_ = new
                                        {
                                            date_detection = false,
                                            _all = new {enabled = true},
                                            dynamic_templates = new object[]
                                                {
                                                    new
                                                        {
                                                            template_strings = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "string",
                                                                    mapping = new
                                                                        {
                                                                            type = "string",
                                                                            store = "no",
                                                                            index = "not_analyzed"
                                                                        }
                                                                },
                                                        },
                                                    new
                                                        {
                                                            template_integer = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "integer",
                                                                    mapping = new
                                                                        {
                                                                            type = "string",
                                                                            store = "no",
                                                                            index = "not_analyzed"
                                                                        }
                                                                },
                                                        },
                                                    new
                                                        {
                                                            template_double = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "double",
                                                                    mapping = new
                                                                        {
                                                                            type = "string",
                                                                            store = "no",
                                                                            index = "not_analyzed"
                                                                        }
                                                                },
                                                        },
                                                    new
                                                        {
                                                            template_boolean = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "boolean",
                                                                    mapping = new
                                                                        {
                                                                            type = "string",
                                                                            store = "no",
                                                                            index = "not_analyzed"
                                                                        }
                                                                },
                                                        }
                                                },
                                            properties = new
                                                {
                                                    Data = new
                                                        {
                                                            type = "object",
                                                            store = "no",
                                                        },
                                                    Meta = new
                                                        {
                                                            properties = new
                                                                {
                                                                    Name = StringTemplate(),
                                                                    Id = StringTemplate(),
                                                                    State = StringTemplate(),
                                                                    ParentTaskId = StringTemplate(),
                                                                    TaskGroupLock = StringTemplate(),
                                                                    Attempts = new {type = "integer"},
                                                                    EnqueueTime = DateTemplate(),
                                                                    MinimalStartTime = DateTemplate(),
                                                                    StartExecutingTime = DateTemplate(),
                                                                    FinishExecutingTime = DateTemplate(),
                                                                    LastModificationTime = DateTemplate(),
                                                                }
                                                        }
                                                }
                                        }
                                },
                            aliases = new Dictionary<string, object>
                                {
                                    {settings.SearchAliasFormat, new {}},
                                    {settings.OldDataAliasFormat, new {}},
                                }
                        }
                    ).ProcessResponse();
            }
        }

        private static object DateTemplate()
        {
            return new {type = "date", format = dateFormat, store = "no"};
        }

        private static object StringTemplate()
        {
            return new {type = "string", store = "no", index = "not_analyzed"};
        }

        private const string dateFormat = "dateOptionalTime";
        public const string DataTemplateSuffix = "data";
        public const string OldDataTemplateSuffix = "old-data";

        private readonly TaskSchemaDynamicSettings settings;
        private readonly IElasticsearchClient elasticsearchClient;

        private static readonly ILog logger = LogManager.GetLogger("TaskSearchIndexSchema");
    }
}