using System;

namespace RemoteQueue.Exceptions
{
    public class TaskHandlerNotFoundException : Exception
    {
        public TaskHandlerNotFoundException(string taskName)
            : base(string.Format("Handlers for task '{0}' not found.", taskName))
        {
        }
    }
}