using System;
using System.Collections.Generic;

using Elasticsearch.Net;

using SKBKontur.Catalogue.Core.ElasticsearchClientExtensions;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Actualizer
{
    public class TaskSearchIndexDataTestService
    {
        public TaskSearchIndexDataTestService(
            IElasticsearchClientFactory elasticsearchClientFactory,
            TaskSchemaDynamicSettings settings)
        {
            this.settings = settings;
            elasticsearchClient = elasticsearchClientFactory.GetClient();
        }

        public void DeleteAll()
        {
            elasticsearchClient.IndicesDelete(settings.LastTicksIndex).ProcessResponse(200, 404);
            //NOTE без этого разрушает индексы и нужен перезапуск ES
            elasticsearchClient.ClearScroll("_all").ProcessResponse(); //todo плохо, мешает чужим поискам
            elasticsearchClient.IndicesDelete(settings.OldDataIndex).ProcessResponse(200, 404);
            elasticsearchClient.IndicesDelete(settings.IndexPrefix + "*").ProcessResponse(200, 404);

            //DeleteDataFromIndices(settings.IndexPrefix + "*");
            //DeleteDataFromIndices(settings.OldDataIndex);

            elasticsearchClient.IndicesDeleteTemplateForAll(settings.TemplateNamePrefix + TaskSearchIndexSchema.DataTemplateSuffix).ProcessResponse(200, 404);
            elasticsearchClient.IndicesDeleteTemplateForAll(settings.TemplateNamePrefix + TaskSearchIndexSchema.OldDataTemplateSuffix).ProcessResponse(200, 404);

            Refresh();
        }

        public void Refresh()
        {
            elasticsearchClient.IndicesRefresh("_all");
        }


        //private void DeleteDataFromIndices(string pattern)
        //{
        //    var searchIndices = FindIndices(pattern);

        //    foreach(var searchIndex in searchIndices)
        //    {
        //        var mapping = elasticsearchClient.IndicesGetMapping<Dictionary<string, MapingItem>>(searchIndex).ProcessResponse();
        //        var types = mapping.Response[searchIndex].mappings.Keys;
        //        foreach(var type in types)
        //            elasticsearchClient.DeleteByQuery(searchIndex, type, new {query = new {match_all = new {}}}).ProcessResponse();
        //    }
        //}

        //private string[] FindIndices(string template)
        //{
        //    var indices = elasticsearchClient.CatIndices(template).ProcessResponse(200, 404);
        //    if(indices.HttpStatusCode == 404)
        //        return new string[0];
        //    return Parse(indices.Response);
        //}

        //private static string[] Parse(string s)
        //{
        //    var strings = s.Split(new[] {"\n"}, StringSplitOptions.None);
        //    var lst = new List<string>();
        //    foreach(var line in strings)
        //    {
        //        var split = line.Split(new[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
        //        if(split.Length > 1)
        //            lst.Add(split[2]);
        //    }
        //    return lst.ToArray();
        //}

        //private static string ToIsoTime(DateTime dt)
        //{
        //    return dt.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK");
        //}

        private readonly TaskSchemaDynamicSettings settings;
        private readonly IElasticsearchClient elasticsearchClient;

        //private class MapingItem
        //{
        //    public Dictionary<string, object> mappings { get; set; }
        //}
    }
}