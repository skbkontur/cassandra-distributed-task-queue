using System;
using System.Threading;

using log4net;

namespace RemoteQueue.LocalTasks.Scheduling
{
    public class SmartTimer : ISmartTimer
    {
        static SmartTimer()
        {
            CreateTimer = (callback, span) => new Timer(callback, null, TimeSpan.Zero, span);
        }

        public SmartTimer(IPeriodicTask task, TimeSpan period)
        {
            logger = LogManager.GetLogger(GetType());
            timer = CreateTimer(x => RunTask(task), period);
            running = false;
            stopped = false;
        }

        #region ISmartTimer Members

        public void StopAndWait(int timeout)
        {
            stopped = true;
            timer.Dispose();
            int currentTimeout = 0;
            while(currentTimeout < timeout)
            {
                currentTimeout += 100;
                Thread.Sleep(100);
                if(!running) return;
            }
            throw new Exception(string.Format("Can not stop timer for {0} ms", timeout));
        }

        #endregion

        public static Func<TimerCallback, TimeSpan, Timer> CreateTimer { get; set; }

        private void RunTask(IPeriodicTask task)
        {
            if(running) return;
            lock(lockObject)
            {
                if(running) return;
                running = true;
            }
            try
            {
                if(stopped)
                {
                    running = false;
                    return;
                }
                logger.DebugFormat("Start run task '{0}'", task.Id);
                task.Run();
                logger.DebugFormat("Finish run task '{0}'", task.Id);
            }
            catch(Exception e)
            {
                logger.Error(string.Format("Error while executing task {0}", task.Id), e);
            }
            running = false;
        }

        private readonly object lockObject = new object();
        private readonly ILog logger;
        private readonly Timer timer;
        private volatile bool running;
        private volatile bool stopped;
    }
}