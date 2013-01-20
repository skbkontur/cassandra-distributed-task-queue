using System;
using System.Linq;

using SKBKontur.Catalogue.Core.Web.Models.DateAndTimeModels;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities.Primitives;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceClient;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.Models.TaskList.SearchPanel;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskMonitoringViewer.ModelBuilders.TaskList
{
    public class TaskListModelBuilder : ITaskListModelBuilder
    {
        public TaskListModelBuilder(IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage,
                                    ITaskMetadataModelBuilder taskMetadataModelBuilder)
        {
            this.remoteTaskQueueMonitoringServiceStorage = remoteTaskQueueMonitoringServiceStorage;
            this.taskMetadataModelBuilder = taskMetadataModelBuilder;
        }

        public TaskListModelData Build(MonitoringSearchRequest searchRequest, MonitoringTaskMetadata[] fullTaskMetaInfos, int totalCount)
        {
            var names = remoteTaskQueueMonitoringServiceStorage.GetDistinctValues(x => true, x => x.Name).Cast<string>().ToArray();
            var states = Enum.GetValues(typeof(TaskState)).Cast<TaskState>().ToArray();

            return new TaskListModelData
                {
                    SearchPanel = new SearchPanelModelData
                        {
                            TaskStates = BuildArray(states, searchRequest.TaskStates),
                            TaskNames = BuildArray(names, searchRequest.TaskNames),
                            TaskId = searchRequest.TaskId,
                            ParentTaskId = searchRequest.ParentTaskId,
                            Ticks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.Ticks.From.UtcToMoscowDateTime()),
                                    To = DateAndTime.Create(searchRequest.Ticks.To.UtcToMoscowDateTime())
                                },
                            StartExecutedTicks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.StartExecutingTicks.From.UtcToMoscowDateTime()),
                                    To = DateAndTime.Create(searchRequest.StartExecutingTicks.To.UtcToMoscowDateTime())
                                },
                            FinishExecutedTicks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.FinishExecutingTicks.From.UtcToMoscowDateTime()),
                                    To = DateAndTime.Create(searchRequest.FinishExecutingTicks.To.UtcToMoscowDateTime())
                                },
                            MinimalStartTicks = new DateTimeRangeModel
                                {
                                    From = DateAndTime.Create(searchRequest.MinimalStartTicks.From.UtcToMoscowDateTime()),
                                    To = DateAndTime.Create(searchRequest.MinimalStartTicks.To.UtcToMoscowDateTime())
                                }
                        },
                    TaskCount = totalCount,
                    TaskModels = fullTaskMetaInfos.Select(taskMetadataModelBuilder.Build).ToArray(),
                };
        }

        private static Pair<T, bool?>[] BuildArray<T>(T[] allowedValues, T[] requestValues)
            where T : IComparable
        {
            var dictionary = allowedValues.ToDictionary(x => x, x => false);
            foreach(var requestValue in requestValues)
            {
                if(dictionary.ContainsKey(requestValue))
                    dictionary[requestValue] = true;
                else
                    dictionary.Add(requestValue, true);
            }
            var array = dictionary.Select(x => new Pair<T, bool?> {Key = x.Key, Value = x.Value}).ToArray();
            Array.Sort(array, (x, y) => x.Key.CompareTo(y.Key));
            return array;
        }

        private readonly IRemoteTaskQueueMonitoringServiceStorage remoteTaskQueueMonitoringServiceStorage;
        private readonly ITaskMetadataModelBuilder taskMetadataModelBuilder;
    }
}