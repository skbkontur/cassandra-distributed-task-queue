using System.Collections.Generic;

using Elasticsearch.Net;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Utils;
using SKBKontur.Catalogue.ServiceLib.Logging;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSearchIndexSchema
    {
        public TaskSearchIndexSchema(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        public void ActualizeTemplate(bool local)
        {
            PutDataTemplate(RtqElasticsearchConsts.TemplateName, RtqElasticsearchConsts.IndexPrefix + "*", local);
            CreateIndexIfNotExists(RtqElasticsearchConsts.OldDataIndex, new {});
            CreateIndexingProgressIndex(RtqElasticsearchConsts.IndexingProgressIndex, local);
        }

        private void CreateIndexingProgressIndex([NotNull] string indexName, bool local)
        {
            CreateIndexIfNotExists(indexName, new
                {
                    settings = Settings(local),
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

        private static object Settings(bool local)
        {
            return local
                       ? new
                           {
                               number_of_shards = 1,
                               number_of_replicas = 0,
                               index = new Dictionary<string, string>()
                           }
                       : new
                           {
                               number_of_shards = 5,
                               number_of_replicas = 1,
                               index = new Dictionary<string, string>
                                   {
                               {"routing.allocation.require._name", "edi-elastic-*"},
                               {"routing.allocation.exclude._name", "edi-elastic-*-i*"}
                                   }
                           };
        }

        private void PutDataTemplate([NotNull] string templateName, [NotNull] string indicesPattern, bool local)
        {
            Log.For(this).LogInfoFormat("Attempt to put data template name '{0}' pattern '{1}'", templateName, indicesPattern);
            elasticsearchClientFactory
                .DefaultClient.Value
                    .IndicesPutTemplateForAll(templateName, new
                        {
                            template = indicesPattern,
                        settings = Settings(local),
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