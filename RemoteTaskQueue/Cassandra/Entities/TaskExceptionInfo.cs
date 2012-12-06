using System;

namespace RemoteQueue.Cassandra.Entities
{
    public class TaskExceptionInfo
    {
        public bool EqualsToException(Exception exception)
        {
            if(ExceptionMessageInfo == null && exception == null)
                return true;
            return ExceptionMessageInfo == exception.ToString();
        }

        public string ExceptionMessageInfo { get; set; }
    }
}