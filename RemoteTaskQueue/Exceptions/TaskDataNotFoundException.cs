using System;

namespace RemoteQueue.Exceptions
{
    public class TaskDataNotFoundException : Exception
    {
        public TaskDataNotFoundException(Type type)
            : base(string.Format("TaskData with type '{0}' not registered", type.FullName))
        {
        }

        public TaskDataNotFoundException(string name)
            : base(string.Format("TaskData with name '{0}' not registered", name))
        {
        }
    }
}