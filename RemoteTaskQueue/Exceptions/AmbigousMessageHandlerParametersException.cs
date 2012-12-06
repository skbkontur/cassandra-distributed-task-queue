using System;

namespace RemoteQueue.Exceptions
{
    public class AmbigousMessageHandlerParametersException : Exception
    {
        public AmbigousMessageHandlerParametersException(string taskType, string contentType, string messageType, string version, string transportType)
            : base(string.Format("MessageHandler с параметрами [taskType = {0}, contentType = '{1}', messageType = '{2}', version = '{3}', transportType = '{4}'] уже был добавлен", taskType, contentType, messageType, version, transportType))
        {
        }
    }
}