using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Api
{
    [InternalAPI]
    public class RemoteTaskQueueSearchResults
    {
        public long TotalCount { get; set; }

        public TaskMetaInformationModel[] TaskMetas { get; set; }
    }
}