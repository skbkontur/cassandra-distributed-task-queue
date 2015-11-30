using System;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Configuration;

using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskMetaProcessor : ITaskMetaProcessor
    {
        public TaskMetaProcessor(
            ITaskDataRegistry taskDataRegistry,
            ITaskDataBlobStorage taskDataStorage,
            ITaskExceptionInfoBlobStorage taskExceptionInfoStorage,
            TaskWriter writer,
            ISerializer serializer,
            ICatalogueStatsDClient statsDClient, ITaskWriteDynamicSettings taskWriteDynamicSettings)
        {
            this.taskDataRegistry = taskDataRegistry;
            this.taskDataStorage = taskDataStorage;
            this.taskExceptionInfoStorage = taskExceptionInfoStorage;
            this.writer = writer;
            this.serializer = serializer;
            this.statsDClient = taskWriteDynamicSettings.GraphitePrefixOrNull != null ?
                                    statsDClient.WithScope(string.Format("{0}.Actualization", taskWriteDynamicSettings.GraphitePrefixOrNull)) :
                                    EmptyStatsDClient.Instance;
        }

        public void IndexMetas(TaskMetaInformation[] batch)
        {
            string[] taskIds = batch.Select(m => m.Id).ToArray();
            var taskDataMap = statsDClient.Timing("ReadTaskDatas", () => taskDataStorage.Read(taskIds));
            var taskExceptionInfoMap = statsDClient.Timing("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(taskIds));
            var taskExceptionInfos = new TaskExceptionInfo[batch.Length];
            var taskDataObjects = new object[batch.Length];
            for(var i = 0; i < batch.Length; i++)
            {
                var taskData = taskDataMap.ContainsKey(batch[i].Id) ? taskDataMap[batch[i].Id] : null;
                Type taskType;
                object taskDataObj = null;
                if(taskDataRegistry.TryGetTaskType(batch[i].Name, out taskType))
                    taskDataObj = serializer.Deserialize(taskType, taskData);
                taskDataObjects[i] = taskDataObj;

                taskExceptionInfos[i] = taskExceptionInfoMap.ContainsKey(batch[i].Id) ? taskExceptionInfoMap[batch[i].Id] : null;
            }
            if(batch.Length > 0)
                statsDClient.Timing("Index", () => writer.IndexBatch(batch, taskExceptionInfos, taskDataObjects));
        }

        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly ITaskExceptionInfoBlobStorage taskExceptionInfoStorage;
        private readonly TaskWriter writer;
        private readonly ISerializer serializer;
        private readonly ICatalogueStatsDClient statsDClient;
    }
}