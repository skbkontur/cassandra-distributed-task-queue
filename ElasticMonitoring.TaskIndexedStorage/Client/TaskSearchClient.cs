using System.Collections.Generic;
using System.Linq;

using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Search;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Search;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Client
{
    public class TaskSearchClient : ITaskSearchClient
    {
        public TaskSearchClient(InternalDataElasticsearchFactory elasticsearchClientFactory, SearchIndexNameFactory searchIndexNameFactory, TaskSearchDynamicSettings taskSearchDynamicSettings)
        {
            this.searchIndexNameFactory = searchIndexNameFactory;
            this.taskSearchDynamicSettings = taskSearchDynamicSettings;
            elasticsearchClient = elasticsearchClientFactory.DefaultClient.Value;
        }

        private static bool NotEmpty(string[] arr)
        {
            return arr != null && arr.Length > 0;
        }

        public TaskSearchResponse Search(TaskSearchRequest taskSearchRequest, int @from, int size)
        {
            var filters = new List<object>
                {
                    new
                        {
                            query_string = new
                                {
                                    query = taskSearchRequest.QueryString,
                                    lenient = true,
                                    allow_leading_wildcard = false
                                },
                        }
                };
            if(NotEmpty(taskSearchRequest.TaskNames))
            {
                filters.Add(new
                    {
                        terms = new Dictionary<string, object>
                            {
                                {"Meta.Name", taskSearchRequest.TaskNames},
                                {"minimum_should_match", 1}
                            }
                    });
            }
            if(NotEmpty(taskSearchRequest.TaskStates))
            {
                filters.Add(new
                    {
                        terms = new Dictionary<string, object>
                            {
                                {"Meta.State", taskSearchRequest.TaskStates},
                                {"minimum_should_match", 1}
                            }
                    });
            }

            var indexForTimeRange = searchIndexNameFactory.GetIndexForTimeRange(
                taskSearchRequest.FromTicksUtc,
                taskSearchRequest.ToTicksUtc,
                taskSearchDynamicSettings.SearchIndexNameFormat);
            var metaResponse =
                elasticsearchClient
                    .Search<SearchResponseNoData>(indexForTimeRange,
                                                  new
                                                      {
                                                          size = size,
                                                          from = from,
                                                          version = true,
                                                          _source = false,
                                                          query = new
                                                              {
                                                                  @bool = new
                                                                      {
                                                                          must = filters
                                                                      }
                                                              },
                                                          sort = new[]
                                                              {
                                                                  new Dictionary<string, object>
                                                                      {
                                                                          {"Meta.MinimalStartTime", new {order = "desc", unmapped_type = "long"}}
                                                                      }
                                                              }
                                                      }, x => x.IgnoreUnavailable(true))
                    .ProcessResponse();
            return new TaskSearchResponse
                {
                    Ids = metaResponse.Response.Hits.Hits.Select(x => x.Id).ToArray(),
                    TotalCount = metaResponse.Response.Hits.Total,
                };
        }

        private readonly SearchIndexNameFactory searchIndexNameFactory;
        private readonly TaskSearchDynamicSettings taskSearchDynamicSettings;
        private readonly IElasticsearchClient elasticsearchClient;
    }
}