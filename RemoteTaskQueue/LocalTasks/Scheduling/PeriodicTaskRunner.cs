using System;
using System.Collections.Generic;

namespace RemoteQueue.LocalTasks.Scheduling
{
    public class PeriodicTaskRunner : IPeriodicTaskRunner
    {
        public void Register(IPeriodicTask task, TimeSpan period)
        {
            lock(lockObject)
            {
                if(!dictionary.ContainsKey(task.Id))
                    dictionary.Add(task.Id, new SmartTimer(task, period));
            }
        }

        public void Unregister(string taskId, int timeout)
        {
            ISmartTimer timer;
            lock(lockObject)
            {
                if(!dictionary.ContainsKey(taskId)) return;
                timer = dictionary[taskId];
                dictionary.Remove(taskId);
            }
            timer.StopAndWait(timeout);
        }

        private readonly Dictionary<string, ISmartTimer> dictionary = new Dictionary<string, ISmartTimer>(StringComparer.OrdinalIgnoreCase);
        private readonly object lockObject = new object();
    }
}