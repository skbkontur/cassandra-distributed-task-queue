using System;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskQueueException : Exception
    {
        public TaskQueueException(string message)
            : base(message)
        {
        }
    }
}