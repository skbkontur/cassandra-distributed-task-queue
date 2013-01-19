using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList
{
    public interface ITaskListModelBuilder
    {
        TaskListModelData Build(MonitoringSearchRequest searchRequest, MonitoringTaskMetadata[] fullTaskMetaInfos, int totalCount);
    }
}