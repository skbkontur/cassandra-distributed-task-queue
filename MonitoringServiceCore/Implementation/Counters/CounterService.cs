using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringDataTypes.MonitoringEntities;
using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters
{
    public class CounterService
    {
        public CounterService(SnapshotsManager snapshotsManager, MetaProvider metaProvider, IProcessedTasksCounter processedTasksCounter)
        {
            this.snapshotsManager = snapshotsManager;
            this.metaProvider = metaProvider;
            this.processedTasksCounter = processedTasksCounter;
            initializer = new ThreadSafeInitializer(InitAndLoad);
        }

        public void SaveSnapshot()
        {
            initializer.EnsureInitialized();
            snapshotsManager.SaveSnapshot();
        }

        public void Restart(long? fromTicksUtc)
        {
            initializer.EnsureInitialized();
            metaProvider.Restart(fromTicksUtc);
            processedTasksCounter.Reset();
        }

        public void Update()
        {
            initializer.EnsureInitialized();
            metaProvider.FetchMetas();
        }

        public TaskCount GetCount()
        {
            var taskCount = processedTasksCounter.GetCount();
            taskCount.StartTicks = metaProvider.StartTicks;
            return taskCount;
        }

        public void Init()
        {
            snapshotsManager.Init();
        }

        private void InitAndLoad()
        {
            Init();
            snapshotsManager.LoadSnapshot();
        }

        private readonly SnapshotsManager snapshotsManager;
        private readonly MetaProvider metaProvider;
        private readonly IProcessedTasksCounter processedTasksCounter;

        private readonly ThreadSafeInitializer initializer;
    }
}