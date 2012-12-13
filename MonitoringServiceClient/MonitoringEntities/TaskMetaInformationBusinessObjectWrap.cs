using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.Core.CommonBusinessObjects;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient.MonitoringEntities
{
    public class TaskMetaInformationBusinessObjectWrap : BusinessObject
    {
        public TaskMetaInformation Info { get; set; }
    }
}