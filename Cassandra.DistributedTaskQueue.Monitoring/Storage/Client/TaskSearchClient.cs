using System.Collections.Generic;
using System.Linq;

using Elasticsearch.Net;

using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Search;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Search;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Storage.Client
{
    public class TaskSearchClient
    {
        public TaskSearchClient(IRtqElasticsearchClient elasticClient)
        {
            this.elasticClient = elasticClient;
            ignoreUnavailableIndices = new SearchRequestParameters {IgnoreUnavailable = true};
            if (elasticClient.UseElastic7)
            {
                ignoreUnavailableIndices.TotalHitsAsInteger = true;
            }
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
            var request = new
                {
                    @from,
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
                };
            var body = PostData.String(JsonConvert.SerializeObject(request));
            var stringResponse = elasticClient.Search<StringResponse>(indexForTimeRange, body, ignoreUnavailableIndices).EnsureSuccess();
            var searchResponse = JsonConvert.DeserializeObject<SearchResponse>(stringResponse.Body);
            return new TaskSearchResponse
                {
                    Ids = searchResponse.Hits.Hits.Select(x => x.Id).ToArray(),
                    TotalCount = searchResponse.Hits.TotalCount
                };
        }

        private readonly IRtqElasticsearchClient elasticClient;
        private readonly SearchRequestParameters ignoreUnavailableIndices;
    }
}