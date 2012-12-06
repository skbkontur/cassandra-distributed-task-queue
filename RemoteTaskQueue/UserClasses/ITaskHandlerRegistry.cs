using System;
using System.Collections.Generic;

using RemoteQueue.Handling;

namespace RemoteQueue.UserClasses
{
    public interface ITaskHandlerRegistry
    {
        KeyValuePair<Type, Func<ITaskHandler>>[] GetAllTaskHandlerCreators();
    }
}