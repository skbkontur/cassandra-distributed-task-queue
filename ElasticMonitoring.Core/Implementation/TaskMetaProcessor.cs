using System;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

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
            ITaskDataStorage taskDataStorage,
            ITaskExceptionInfoStorage taskExceptionInfoStorage,
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

        public void IndexMetas([NotNull] TaskMetaInformation[] batch)
        {
            if(!batch.Any())
                return;
            var taskDatas = statsDClient.Timing("ReadTaskDatas", () => taskDataStorage.Read(batch));
            var taskExceptionInfos = statsDClient.Timing("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(batch));
            var enrichedBatch = new Tuple<TaskMetaInformation, TaskExceptionInfo[], object>[batch.Length];
            for(var i = 0; i < batch.Length; i++)
            {
                var taskMeta = batch[i];
                byte[] taskData;
                object taskDataObj = null;
                if(taskDatas.TryGetValue(taskMeta.Id, out taskData))
                {
                    Type taskType;
                    if(taskDataRegistry.TryGetTaskType(taskMeta.Name, out taskType))
                        taskDataObj = serializer.Deserialize(taskType, taskData);
                }
                enrichedBatch[i] = Tuple.Create(taskMeta, taskExceptionInfos[taskMeta.Id], taskDataObj);
            }
            statsDClient.Timing("Index", () => writer.IndexBatch(enrichedBatch));
        }

        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly TaskWriter writer;
        private readonly ISerializer serializer;
        private readonly ICatalogueStatsDClient statsDClient;
    }
}