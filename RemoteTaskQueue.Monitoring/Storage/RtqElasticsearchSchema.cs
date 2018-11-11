using Elasticsearch.Net;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Objects.Json;

namespace RemoteTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchSchema
    {
        public RtqElasticsearchSchema(RtqElasticsearchClientFactory elasticClientFactory)
        {
            elasticClient = elasticClientFactory.DefaultClient.Value;
        }

        public void Actualize(bool local, bool bulkLoad)
        {
            var indexSettings = new {settings = GetIndexingProgressIndexSettings(local, bulkLoad)};
            elasticClient.IndicesCreate<StringResponse>(RtqElasticsearchConsts.IndexingProgressIndexName, PostData.String(indexSettings.ToJson()), allowResourceAlreadyExistsStatus).EnsureSuccess();

            var templateSettings = GetTaskIndicesTemplateSettings(local, bulkLoad);
            elasticClient.IndicesPutTemplateForAll<StringResponse>(RtqElasticsearchConsts.TemplateName, PostData.String(templateSettings.ToJson())).EnsureSuccess();
        }

        [NotNull]
        private static object GetIndexingProgressIndexSettings(bool local, bool bulkLoad)
        {
            return new
                {
                    index = new
                        {
                            number_of_shards = 6,
                            number_of_replicas = local || bulkLoad ? 0 : 1,
                            refresh_interval = bulkLoad ? "-1" : (local ? "1s" : "10s"),
                            merge = new {scheduler = new {max_thread_count = 1}}, // see https://www.elastic.co/guide/en/elasticsearch/reference/current/index-modules-merge.html
                            query = new
                                {
                                    default_field = "DataAsText",
                                }
                        }
                };
        }

        private static object GetTaskIndicesTemplateSettings(bool local, bool bulkLoad)
        {
            return new
                {
                    index_patterns = RtqElasticsearchConsts.AllIndicesWildcard,
                    settings = GetIndexingProgressIndexSettings(local, bulkLoad),
                    mappings = new
                        {
                            doc_type_is_deprecated = new
                                {
                                    date_detection = false,
                                    dynamic_templates = new object[]
                                        {
                                            new
                                                {
                                                    template_strings = new
                                                        {
                                                            path_match = "Data.*",
                                                            match_mapping_type = "string",
                                                            mapping = KeywordTypeWithCopy(),
                                                        }
                                                },
                                            new
                                                {
                                                    template_integer = new
                                                        {
                                                            path_match = "Data.*",
                                                            match_mapping_type = "long",
                                                            mapping = KeywordType()
                                                        }
                                                },
                                            new
                                                {
                                                    template_double = new
                                                        {
                                                            path_match = "Data.*",
                                                            match_mapping_type = "double",
                                                            mapping = KeywordType()
                                                        }
                                                },
                                            new
                                                {
                                                    template_boolean = new
                                                        {
                                                            path_match = "Data.*",
                                                            match_mapping_type = "boolean",
                                                            mapping = KeywordType()
                                                        }
                                                }
                                        },
                                    properties = new
                                        {
                                            Data = new
                                                {
                                                    type = "object"
                                                },
                                            DataAsText = new
                                                {
                                                    type = "text",
                                                    store = false,
                                                    index = true
                                                },
                                            ExceptionInfo = new
                                                {
                                                    type = "text",
                                                    store = false,
                                                    index = true,
                                                    copy_to = "DataAsText",
                                                },
                                            Meta = new
                                                {
                                                    properties = new
                                                        {
                                                            Name = KeywordTypeWithCopy(),
                                                            Id = KeywordTypeWithCopy(),
                                                            State = KeywordTypeWithCopy(),
                                                            ParentTaskId = KeywordTypeWithCopy(),
                                                            TaskGroupLock = KeywordTypeWithCopy(),
                                                            Attempts = new {type = "integer", store = false},
                                                            EnqueueTime = DateType(),
                                                            MinimalStartTime = DateType(),
                                                            StartExecutingTime = DateType(),
                                                            FinishExecutingTime = DateType(),
                                                            LastModificationTime = DateType(),
                                                            ExpirationTime = DateType()
                                                        }
                                                }
                                        }
                                }
                        }
                };
        }

        [NotNull]
        private static object DateType()
        {
            return new {type = "date", store = false, format = "dateOptionalTime"};
        }

        [NotNull]
        private static object KeywordType()
        {
            return new {type = "keyword", store = false, index = true};
        }

        [NotNull]
        private static object KeywordTypeWithCopy()
        {
            return new
                {
                    type = "keyword",
                    store = false,
                    index = true,
                    copy_to = "DataAsText",
                };
        }

        private readonly IElasticLowLevelClient elasticClient;

        private readonly CreateIndexRequestParameters allowResourceAlreadyExistsStatus = new CreateIndexRequestParameters
            {
                RequestConfiguration = new RequestConfiguration {AllowedStatusCodes = new[] {400}}
            };
    }
}