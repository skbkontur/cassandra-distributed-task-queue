using System;
using System.Linq;

using GroBuf;

using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.BlobStorages;
using RemoteQueue.Configuration;

using RemoteTaskQueue.Monitoring.Storage.Utils;
using RemoteTaskQueue.Monitoring.Storage.Writing;

using SKBKontur.Catalogue.ServiceLib.Logging;

namespace RemoteTaskQueue.Monitoring.Indexer
{
    public class TaskMetaProcessor : ITaskMetaProcessor
    {
        public TaskMetaProcessor(
            ITaskDataRegistry taskDataRegistry,
            ITaskDataStorage taskDataStorage,
            ITaskExceptionInfoStorage taskExceptionInfoStorage,
            TaskWriter writer,
            ISerializer serializer,
            IRtqElasticsearchIndexerGraphiteReporter graphiteReporter)
        {
            this.taskDataRegistry = taskDataRegistry;
            this.taskDataStorage = taskDataStorage;
            this.taskExceptionInfoStorage = taskExceptionInfoStorage;
            this.writer = writer;
            this.serializer = serializer;
            this.graphiteReporter = graphiteReporter;
        }

        public void IndexMetas([NotNull] TaskMetaInformation[] batch)
        {
            if(!batch.Any())
                return;
            var taskDatas = graphiteReporter.ReportTiming("ReadTaskDatas", () => taskDataStorage.Read(batch));
            var taskExceptionInfos = graphiteReporter.ReportTiming("ReadTaskExceptionInfos", () => taskExceptionInfoStorage.Read(batch));
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
                        taskDataObj = DeserializeTaskDataSafe(taskType, taskData, taskMeta);
                }
                enrichedBatch[i] = Tuple.Create(taskMeta, taskExceptionInfos[taskMeta.Id], taskDataObj);
            }
            graphiteReporter.ReportTiming("IndexBatch", () => writer.IndexBatch(enrichedBatch));
        }

        private object DeserializeTaskDataSafe(Type taskType, byte[] taskData, TaskMetaInformation taskMetaInformation)
        {
            try
            {
                return serializer.Deserialize(taskType, taskData);
            }
            catch(Exception e)
            {
                Log.For(this).LogErrorFormat(e, "Data deserialization error. Taskmeta {0}", taskMetaInformation);
                return null;
            }
        }

        private readonly ITaskDataRegistry taskDataRegistry;
        private readonly ITaskDataStorage taskDataStorage;
        private readonly ITaskExceptionInfoStorage taskExceptionInfoStorage;
        private readonly TaskWriter writer;
        private readonly ISerializer serializer;
        private readonly IRtqElasticsearchIndexerGraphiteReporter graphiteReporter;
    }
}