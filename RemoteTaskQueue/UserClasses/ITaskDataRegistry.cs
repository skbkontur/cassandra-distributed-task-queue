using System;
using System.Collections.Generic;

namespace RemoteQueue.UserClasses
{
    public interface ITaskDataRegistry
    {
        KeyValuePair<Type, string>[] GetAllTaskDataInfos();
    }
}