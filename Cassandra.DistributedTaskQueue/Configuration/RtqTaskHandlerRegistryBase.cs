using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    public abstract class RtqTaskHandlerRegistryBase : IRtqTaskHandlerRegistry
    {
        protected RtqTaskHandlerRegistryBase(IRtqTaskDataRegistry taskDataRegistry)
        {
            this.taskDataRegistry = taskDataRegistry;
        }

        protected void Register<THandler>([NotNull] Func<THandler> createTaskHandler)
            where THandler : IRtqTaskHandler
        {
            Register(typeof(THandler), () => createTaskHandler());
        }

        [UsedImplicitly]
        protected void Register(Type handlerType, Func<IRtqTaskHandler> createTaskHandler)
        {
            var taskDataType = TryGetTaskDataType(handlerType);
            if (taskDataType == null)
                throw new InvalidOperationException($"Type '{handlerType.FullName}' doesn't implement 'TaskHandler<TTaskData>'");
            var taskName = taskDataRegistry.GetTaskName(taskDataType);
            if (taskHandlerCreatorsByTaskName.ContainsKey(taskName))
                throw new InvalidOperationException($"There are at least two handlers for task '{taskName}': '{taskHandlerCreatorsByTaskName[taskName].HandlerType}' and '{handlerType}'");
            var taskHandlerCreator = new TaskHandlerCreator(handlerType, createTaskHandler);
            taskHandlerCreatorsByTaskName.Add(taskName, taskHandlerCreator);
        }

        [NotNull]
        public string[] GetAllTaskTopicsToHandle()
        {
            return taskHandlerCreatorsByTaskName.Select(x => taskDataRegistry.GetTaskTopic(x.Key)).Distinct().ToArray();
        }

        public bool ContainsHandlerFor([NotNull] string taskName)
        {
            return taskHandlerCreatorsByTaskName.ContainsKey(taskName);
        }

        [NotNull]
        public IRtqTaskHandler CreateHandlerFor([NotNull] string taskName)
        {
            if (!taskHandlerCreatorsByTaskName.TryGetValue(taskName, out var taskHandlerCreator))
                throw new InvalidOperationException(string.Format("Handler not found for taskName: {0}", taskName));
            return taskHandlerCreator.CreateTaskHandler();
        }

        [CanBeNull]
        private static Type TryGetTaskDataType([CanBeNull] Type handlerType)
        {
            if (handlerType == null)
                return null;
            if (handlerType.IsGenericType && handlerType.GetGenericTypeDefinition() == typeof(RtqTaskHandler<>))
            {
                var taskDataType = handlerType.GetGenericArguments().Single();
                if (!typeof(IRtqTaskData).IsAssignableFrom(taskDataType))
                    throw new InvalidOperationException(string.Format("Generic argument of type '{0}' does not implement IRtqTaskData", handlerType.FullName));
                return taskDataType;
            }
            return TryGetTaskDataType(handlerType.BaseType);
        }

        private readonly IRtqTaskDataRegistry taskDataRegistry;
        private readonly Dictionary<string, TaskHandlerCreator> taskHandlerCreatorsByTaskName = new Dictionary<string, TaskHandlerCreator>();

        private class TaskHandlerCreator
        {
            public TaskHandlerCreator([NotNull] Type handlerType, [NotNull] Func<IRtqTaskHandler> createTaskHandler)
            {
                HandlerType = handlerType;
                CreateTaskHandler = createTaskHandler;
            }

            [NotNull]
            public Type HandlerType { get; }

            [NotNull]
            public Func<IRtqTaskHandler> CreateTaskHandler { get; }
        }
    }
}