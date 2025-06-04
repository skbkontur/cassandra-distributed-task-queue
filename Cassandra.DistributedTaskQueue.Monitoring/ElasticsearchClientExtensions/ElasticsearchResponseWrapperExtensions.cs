﻿using System;
using System.Linq;
using System.Text;

using Elasticsearch.Net;

using JetBrains.Annotations;

using Newtonsoft.Json;

using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions.Responses.Bulk;
using SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.ElasticsearchClientExtensions
{
    // todo (andrew, 11.11.2018): maybe enable ConnectionConfiguration.ThrowExceptions() instead of all this boilerplate
    internal static class ElasticsearchResponseWrapperExtensions
    {
        [NotNull]
        public static T EnsureSuccess<T>([NotNull] this T response)
            where T : ElasticsearchResponseBase
        {
            if (!response.Success)
                throw new InvalidOperationException(response.ExtendErrorMessageWithElasticInfo("Unsuccessful response"), response.OriginalException);
            return response;
        }

        public static void DieIfBulkRequestFailed([NotNull] this StringResponse response)
        {
            response.EnsureSuccess();
            var bulkResponse = JsonConvert.DeserializeObject<BulkResponse>(response.Body);
            if (bulkResponse.HasErrors)
            {
                var innerExceptions = bulkResponse
                                      .Items
                                      .Select(x => x.Index ?? x.Create ?? x.Update ?? x.Delete)
                                      .Select(CreateExceptionIfError)
                                      .Where(x => x != null)
                                      .ToList();
                var message = response.ExtendErrorMessageWithElasticInfo($"Bulk request failure [{innerExceptions.Count}]");
                throw new InvalidOperationException(message, new AggregateException(innerExceptions.Take(100)));
            }
        }

        [NotNull]
        private static string ExtendErrorMessageWithElasticInfo<T>([CanBeNull] this T response, string errorMessage)
            where T : IElasticsearchResponse
        {
            var fullErrorMessage = new StringBuilder($"ElasticSearch error: '{errorMessage}'").AppendLine();

            // note ConnectionConfiguration.EnableMetrics() is gone (https://github.com/elastic/elasticsearch-net/issues/1762)
            // note IElasticSearchResponse.NumberOfRetries is gone (https://stackoverflow.com/questions/38602670/what-is-the-elasticsearch-net-2-x-equivalent-for-ielasticsearchresponse-numberof)

            if (response != null)
            {
                if (response.TryGetServerErrorReason(out var serverErrorReason))
                    fullErrorMessage.AppendLine($"ServerErrorReason: '{serverErrorReason}'");

                fullErrorMessage.AppendLine($"For response: '{response}'");
            }

            return fullErrorMessage.ToString();
        }

        [CanBeNull]
        private static Exception CreateExceptionIfError([NotNull] ResponseBase responseItem, int requestNumber)
        {
            return responseItem.Status >= 400 && responseItem.Status < 600
                       ? new InvalidOperationException($"Request number #{requestNumber} failed: '{responseItem.ToPrettyJson()}'")
                       : null;
        }
    }
}