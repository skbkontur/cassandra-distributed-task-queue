using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using RemoteTaskQueue.Monitoring.Storage.Search;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;
using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions.Responses.Search;

namespace RemoteTaskQueue.Monitoring.Storage.Client
{
    public class TaskSearchClient
    {
        public TaskSearchClient(RtqElasticsearchClientFactory elasticsearchClientFactory)
        {
            this.elasticsearchClientFactory = elasticsearchClientFactory;
        }

        [NotNull]
        public TaskSearchResponse Search([NotNull] TaskSearchRequest taskSearchRequest, int from, int size)
        {
            var mustClause = new List<object>
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
            var matchByName = false;
            var shouldClauses = new List<object>();
            foreach (var taskName in taskSearchRequest.TaskNames ?? new string[0])
            {
                matchByName = true;
                shouldClauses.Add(new
                    {
                        term = new Dictionary<string, object>
                            {
                                {"Meta.Name", taskName},
                            }
                    });
            }
            var matchByState = false;
            foreach (var taskState in taskSearchRequest.TaskStates ?? new string[0])
            {
                matchByState = true;
                shouldClauses.Add(new
                    {
                        term = new Dictionary<string, object>
                            {
                                {"Meta.State", taskState},
                            }
                    });
            }
            var query = shouldClauses.Any()
                            ? new
                                {
                                    must = mustClause,
                                    should = shouldClauses,
                                    minimum_should_match = (matchByName ? 1 : 0) + (matchByState ? 1 : 0)
                                }
                            : (object)new
                                {
                                    must = mustClause
                                };

            var indexForTimeRange = SearchIndexNameFactory.GetIndexForTimeRange(taskSearchRequest.FromTicksUtc, taskSearchRequest.ToTicksUtc);
            var metaResponse = elasticsearchClientFactory
                .DefaultClient.Value
                .Search<SearchResponse>(indexForTimeRange,
                                        new
                                            {
                                                from,
                                                size,
                                                version = true,
                                                _source = false,
                                                query = new
                                                    {
                                                        @bool = query
                                                    },
                                                sort = new[]
                                                    {
                                                        new Dictionary<string, object>
                                                            {
                                                                {"Meta.EnqueueTime", new {order = "desc", unmapped_type = "long"}}
                                                            }
                                                    }
                                            }, x => x.IgnoreUnavailable(true))
                .ProcessResponse();
            return new TaskSearchResponse
                {
                    Ids = metaResponse.Response.Hits.Hits.Select(x => x.Id).ToArray(),
                    TotalCount = metaResponse.Response.Hits.TotalCount
                };
        }

        private readonly RtqElasticsearchClientFactory elasticsearchClientFactory;
    }
}