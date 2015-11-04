using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace RemoteQueue.Configuration
{
    public interface ITaskDataRegistry
    {
        [NotNull]
        KeyValuePair<Type, string>[] GetAllTaskDataInfos();
    }
}