using log4net;

using RemoteQueue.LocalTasks.Scheduling;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.Utils;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Scheduler
{
    public class TaskCounterServiceSchedulableRunner : ITaskCounterServiceSchedulableRunner
    {
        public TaskCounterServiceSchedulableRunner(
            IPeriodicTaskRunner periodicTaskRunner,
            ICounterController counterController,
            GraphitePoster graphitePoster)
        {
            this.periodicTaskRunner = periodicTaskRunner;
            this.counterController = counterController;
            this.graphitePoster = graphitePoster;
        }

        public void Stop()
        {
            if(worked)
            {
                lock(lockObject)
                {
                    if(worked)
                    {
                        periodicTaskRunner.Unregister(counterTaskId, 15000);
                        periodicTaskRunner.Unregister(counterGraphiteTaskId, 15000);
                        worked = false;
                        logger.LogInfoFormat("Stop TaskCounterServiceSchedulableRunner");
                    }
                }
            }
        }

        public void Start()
        {
            if(!worked)
            {
                lock(lockObject)
                {
                    if(!worked)
                    {
                        periodicTaskRunner.Register(new ActionPeriodicTask(() => counterController.ProcessNewEvents(), counterTaskId), CounterSettings.CounterUpdateInterval);
                        periodicTaskRunner.Register(new ActionPeriodicTask(() => graphitePoster.PostData(), counterGraphiteTaskId), CounterSettings.GraphitePostInterval);
                        worked = true;
                        logger.LogInfoFormat("Start TaskCounterServiceSchedulableRunner");
                    }
                }
            }
        }

        private const string counterTaskId = "counterTaskId";
        private const string counterGraphiteTaskId = "counterGraphiteTaskId";

        private readonly object lockObject = new object();
        private readonly IPeriodicTaskRunner periodicTaskRunner;
        private readonly ICounterController counterController;
        private readonly GraphitePoster graphitePoster;

        private volatile bool worked;

        private readonly ILog logger = LogManager.GetLogger(typeof(TaskCounterServiceSchedulableRunner));
    }
}