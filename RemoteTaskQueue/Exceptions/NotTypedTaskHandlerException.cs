using System;

namespace RemoteQueue.Exceptions
{
    public class NotTypedTaskHandlerException : Exception
    {
        public NotTypedTaskHandlerException(Type type)
            : base(string.Format("Type '{0}' doesn't implement 'TaskHander<>'", type.FullName))
        {
        }
    }
}