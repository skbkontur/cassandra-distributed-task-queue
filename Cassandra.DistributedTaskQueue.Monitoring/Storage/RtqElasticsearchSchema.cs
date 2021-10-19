using Elasticsearch.Net;
using Elasticsearch.Net.Specification.IndicesApi;

using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage
{
    public class RtqElasticsearchSchema
    {
        public RtqElasticsearchSchema(IRtqElasticsearchClient elasticClient)
        {
            this.elasticClient = elasticClient;
        }

        public void Actualize(bool local, bool bulkLoad)
        {
            var indexSettings = new {settings = GetIndexingProgressIndexSettings(local, bulkLoad)};
            var indexSettingsPostData = PostData.String(JsonConvert.SerializeObject(indexSettings));
            elasticClient.Indices.Create<StringResponse>(RtqElasticsearchConsts.IndexingProgressIndexName, indexSettingsPostData, allowResourceAlreadyExistsStatus).EnsureSuccess();

            var templateSettings = GetTaskIndicesTemplateSettings(local, bulkLoad);
            var templateSettingsPostData = PostData.String(JsonConvert.SerializeObject(templateSettings));
            elasticClient.Indices.PutTemplateForAll<StringResponse>(RtqElasticsearchConsts.TemplateName, templateSettingsPostData, new PutIndexTemplateRequestParameters {IncludeTypeName = true}).EnsureSuccess();
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

        [NotNull]
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
                                                            ExpirationTime = DateType(),
                                                            LastExecutionDurationInMs = DoubleType(),
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
        private static object DoubleType()
        {
            return new {type = "double", store = false};
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

        private readonly IRtqElasticsearchClient elasticClient;

        private readonly CreateIndexRequestParameters allowResourceAlreadyExistsStatus = new CreateIndexRequestParameters
            {
                RequestConfiguration = new RequestConfiguration {AllowedStatusCodes = new[] {400}}
            };
    }
}