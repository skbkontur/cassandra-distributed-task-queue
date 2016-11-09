using JetBrains.Annotations;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling;

using SKBKontur.Catalogue.Core.InternalApi.Core;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.Api
{
    [InternalAPI]
    public class RemoteTaskInfoModel
    {
        [NotNull]
        public TaskMetaInformationModel TaskMeta { get; set; }

        [NotNull]
        public ITaskData TaskData { get; set; }

        [NotNull]
        public TaskExceptionInfo[] ExceptionInfos { get; set; }
    }
}