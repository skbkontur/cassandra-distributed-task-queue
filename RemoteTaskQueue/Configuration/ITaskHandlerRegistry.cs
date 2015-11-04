using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Handling;

namespace RemoteQueue.Configuration
{
    public interface ITaskHandlerRegistry
    {
        [NotNull]
        KeyValuePair<Type, Func<ITaskHandler>>[] GetAllTaskHandlerCreators();
    }
}