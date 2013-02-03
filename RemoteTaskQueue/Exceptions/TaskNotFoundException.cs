using System;

namespace RemoteQueue.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException(string taskId)
            : base(string.Format("Task with Id='{0}' was not found", taskId))
        {
        }
    }
}