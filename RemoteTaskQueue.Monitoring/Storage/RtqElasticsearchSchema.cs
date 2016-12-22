using System.Collections.Generic;

using Elasticsearch.Net;

using JetBrains.Annotations;

using RemoteTaskQueue.Monitoring.Storage.Utils;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchSchema
    {
        public RtqElasticsearchSchema(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        public void Actualize(bool local, bool bulkLoad)
        {
            PutDataTemplate(RtqElasticsearchConsts.TemplateName, RtqElasticsearchConsts.IndexPrefix + "*", local, bulkLoad);
            CreateIndexIfNotExists(RtqElasticsearchConsts.OldDataIndex, new {});
            CreateIndexingProgressIndex(RtqElasticsearchConsts.IndexingProgressIndex, local, bulkLoad);
        }

        private void CreateIndexingProgressIndex([NotNull] string indexName, bool local, bool bulkLoad)
        {
            CreateIndexIfNotExists(indexName, new
                {
                    settings = Settings(local, bulkLoad),
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
                                                    index = false
                                                }
                                        }
                                }
                        }
                });
        }

        private void CreateIndexIfNotExists([NotNull] string indexName, [NotNull] object body)
        {
            Log.For(this).LogInfoFormat("Attempt to create Index {0}", indexName);

            var elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
            if(elasticsearchClient.IndicesExists(indexName).ProcessResponse(200, 404).HttpStatusCode == 404)
            {
                Log.For(this).LogInfoFormat("Index not exists - creating {0}", indexName);
                elasticsearchClient.IndicesCreate(indexName, body).ProcessResponse();
                elasticsearchClient.ClusterHealth(indexName, p => p.WaitForStatus(WaitForStatus.Green)).ProcessResponse();
            }
            else
                Log.For(this).LogInfoFormat("Index already exists");
        }

        [NotNull]
        private static object Settings(bool local, bool bulkLoad)
        {
            return new
                           {
                    index = new
                        {
                            number_of_shards = bulkLoad ? 3 : 1,
                            number_of_replicas = local || bulkLoad ? 0 : 1,
                            refresh_interval = bulkLoad ? "-1" : "1s",
                           }
                           };
        }

        private void PutDataTemplate([NotNull] string templateName, [NotNull] string indicesPattern, bool local, bool bulkLoad)
        {
            Log.For(this).LogInfoFormat("Attempt to put data template name '{0}' pattern '{1}'", templateName, indicesPattern);
            elasticsearchClientFactory
                .DefaultClient.Value
                    .IndicesPutTemplateForAll(templateName, new
                        {
                            template = indicesPattern,
                        settings = Settings(local, bulkLoad),
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
                                                                            type = "keyword",
                                                                            store = false,
                                                                            index = true,
                                                                        }
                                                                },
                                                        },
                                                    new
                                                        {
                                                            template_integer = new
                                                                {
                                                                    path_match = "Data.*",
                                                                    match_mapping_type = "long",
                                                                    mapping = new
                                                                        {
                                                                            type = "keyword",
                                                                            store = false,
                                                                            index = true,
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
                                                                            type = "keyword",
                                                                            store = false,
                                                                            index = true,
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
                                                                            type = "keyword",
                                                                            store = false,
                                                                            index = true,
                                                                        }
                                                                },
                                                        }
                                                },
                                            properties = new
                                                {
                                                    Data = new
                                                        {
                                                            type = "object"
                                                        },
                                                    Meta = new
                                                        {
                                                            properties = new
                                                                {
                                                                    Name = KeywordTemplate(),
                                                                    Id = KeywordTemplate(),
                                                                    State = KeywordTemplate(),
                                                                    ParentTaskId = KeywordTemplate(),
                                                                    TaskGroupLock = KeywordTemplate(),
                                                                    Attempts = new {type = "integer", store = false},
                                                                    EnqueueTime = DateTemplate(),
                                                                    MinimalStartTime = DateTemplate(),
                                                                    StartExecutingTime = DateTemplate(),
                                                                    FinishExecutingTime = DateTemplate(),
                                                                    LastModificationTime = DateTemplate(),
                                                                    Exception = TextTemplate(),
                                                                    ExpirationTime = DateTemplate()
                                                                }
                                                        }
                                                }
                                        }
                                },
                            aliases = new Dictionary<string, object>
                                {
                                {RtqElasticsearchConsts.SearchAliasFormat, new {}},
                                {RtqElasticsearchConsts.OldDataAliasFormat, new {}},
                                }
                        }
                    ).ProcessResponse();
            }

        [NotNull]
        private static object DateTemplate()
        {
            return new {type = "date", store = false, format = "dateOptionalTime"};
        }

        [NotNull]
        private static object TextTemplate()
        {
            return new {type = "text", store = false, index = true};
        }

        [NotNull]
        private static object KeywordTemplate()
        {
            return new {type = "keyword", store = false, index = true};
        }

        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}