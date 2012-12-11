using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders
{
    public interface ITaskMetaInfoModelBuilder
    {
        TaskMetaInfoModel Build(TaskMetaInformation meta);
    }
}