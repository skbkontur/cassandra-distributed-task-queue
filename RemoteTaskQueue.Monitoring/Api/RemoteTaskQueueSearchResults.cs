using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace RemoteTaskQueue.Monitoring.Api
{
    [InternalAPI]
    public class RemoteTaskQueueSearchResults
    {
        public long TotalCount { get; set; }

        public TaskMetaInformationModel[] TaskMetas { get; set; }
    }
}