using System;
using System.Collections.Generic;

using RemoteQueue.Handling;

namespace RemoteQueue.UserClasses
{
    public abstract class TaskDataRegistryBase
    {
        public KeyValuePair<Type, string>[] GetAllTaskDataInfos()
        {
            return list.ToArray();
        }

        protected void Register<T>(string taskName) where T : ITaskData
        {
            list.Add(new KeyValuePair<Type, string>(typeof(T), taskName));
        }

        private readonly List<KeyValuePair<Type, string>> list = new List<KeyValuePair<Type, string>>();
    }
}