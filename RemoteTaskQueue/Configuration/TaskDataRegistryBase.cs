using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using RemoteQueue.Handling;

namespace RemoteQueue.Configuration
{
    public abstract class TaskDataRegistryBase : ITaskDataRegistry
    {
        [NotNull]
        public KeyValuePair<Type, string>[] GetAllTaskDataInfos()
        {
            return list.ToArray();
        }

        protected void Register<T>([NotNull] string taskName) where T : ITaskData
        {
            list.Add(new KeyValuePair<Type, string>(typeof(T), taskName));
        }

        private readonly List<KeyValuePair<Type, string>> list = new List<KeyValuePair<Type, string>>();
    }
}