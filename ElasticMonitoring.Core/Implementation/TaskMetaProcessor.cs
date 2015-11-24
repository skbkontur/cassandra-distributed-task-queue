using System;
using System.Linq;

using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.Graphite.Client.StatsD;
using SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Core.Implementation
{
    public class TaskMetaProcessor : ITaskMetaProcessor
    {
        public TaskMetaProcessor(
            ITaskDataTypeToNameMapper taskDataTypeToNameMapper,
            ITaskDataBlobStorage taskDataStorage,
            ITaskExceptionInfoBlobStorage taskExceptionInfoStorage,
            TaskWriter writer,
            ISerializer serializer,
            ICatalogueStatsDClient statsDClient, ITaskWriteDynamicSettings taskWriteDynamicSettings)
        {
            this.taskDataTypeToNameMapper = taskDataTypeToNameMapper;
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
            var taskDatas = statsDClient.Timing("ReadTaskDatas", () => taskDataStorage.ReadQuiet(taskIds));
            var taskExceptionInfos = statsDClient.Timing("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.ReadQuiet(taskIds));
            var taskDataObjects = new object[taskDatas.Length];
            for(var i = 0; i < batch.Length; i++)
            {
                var taskData = taskDatas[i];
                Type taskType;
                object taskDataObj = null;
                if(taskDataTypeToNameMapper.TryGetTaskType(batch[i].Name, out taskType))
                    taskDataObj = serializer.Deserialize(taskType, taskData);
                taskDataObjects[i] = taskDataObj;
            }
            if(batch.Length > 0)
                statsDClient.Timing("Index", () => writer.IndexBatch(batch, taskExceptionInfos, taskDataObjects));
        }

        private readonly ITaskDataTypeToNameMapper taskDataTypeToNameMapper;
        private readonly ITaskDataBlobStorage taskDataStorage;
        private readonly ITaskExceptionInfoBlobStorage taskExceptionInfoStorage;
        private readonly TaskWriter writer;
        private readonly ISerializer serializer;
        private readonly ICatalogueStatsDClient statsDClient;
    }
}