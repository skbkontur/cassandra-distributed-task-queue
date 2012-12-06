using System;

namespace RemoteQueue.Exceptions
{
    public class TaskNameDuplicateException : Exception
    {
        public TaskNameDuplicateException(string taskName)
            : base(string.Format("Duplicate task name '{0}'.", taskName))
        {
        }
    }
}