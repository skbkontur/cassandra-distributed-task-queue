using log4net;

using RemoteTaskQueue.TaskCounter.Implementation;
using RemoteTaskQueue.TaskCounter.Implementation.Utils;

using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteTaskQueue.TaskCounter.Scheduler
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
            if(started)
            {
                lock(lockObject)
                {
                    if(started)
                    {
                        periodicTaskRunner.Unregister(counterTaskId, 15000);
                        periodicTaskRunner.Unregister(counterGraphiteTaskId, 15000);
                        started = false;
                        logger.LogInfoFormat("Stop TaskCounterServiceSchedulableRunner");
                    }
                }
            }
        }

        public void Start()
        {
            if(!started)
            {
                lock(lockObject)
                {
                    if(!started)
                    {
                        periodicTaskRunner.Register(new ActionPeriodicTask(counterTaskId, () => counterController.ProcessNewEvents()), CounterSettings.CounterUpdateInterval);
                        periodicTaskRunner.Register(new ActionPeriodicTask(counterGraphiteTaskId, () => graphitePoster.PostData()), CounterSettings.GraphitePostInterval);
                        started = true;
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

        private volatile bool started;

        private readonly ILog logger = LogManager.GetLogger(typeof(TaskCounterServiceSchedulableRunner));
    }
}