using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Client
{
    public interface IRemoteTaskQueueTaskCounterClient
    {
        [NotNull]
        TaskCount GetProcessingTaskCount();

        void RestartProcessingTaskCounter(DateTime? fromTime);
    }
}