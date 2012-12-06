using System;

namespace RemoteQueue.Exceptions
{
    public class TooManyTaskHandlersException : Exception
    {
        public TooManyTaskHandlersException(string taskName, Type taskHandlerType1, Type taskHandlerType2)
            : base(string.Format("There are at least two handlers for task '{0}': '{1}' and '{2}'", taskName, taskHandlerType1.FullName, taskHandlerType2.FullName))
        {
        }
    }
}