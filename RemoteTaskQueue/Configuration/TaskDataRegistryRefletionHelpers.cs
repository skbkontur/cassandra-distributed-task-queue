using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Configuration
{
    public static class TaskDataRegistryRefletionHelpers
    {
        [NotNull]
        public static string GetTaskName([NotNull] this Type taskDataType)
        {
            var taskNameAttribute = GetAttributesForType<TaskNameAttribute>(taskDataType).SingleOrDefault();
            if (taskNameAttribute == null)
                throw new InvalidProgramStateException($"TaskName attribute not found for: {taskDataType.FullName}");
            return taskNameAttribute.TaskName;
        }

        [CanBeNull]
        public static string TryGetTaskTopic([NotNull] this Type taskDataType, bool taskTopicIsRequired)
        {
            var taskTopicAttribute = GetAllTypesToSearchForAttributes(taskDataType).SelectMany(GetAttributesForType<TaskTopicAttribute>).SingleOrDefault();
            if (taskTopicIsRequired && taskTopicAttribute == null)
                throw new InvalidProgramStateException($"TaskTopic attribute not found for: {taskDataType.FullName}");
            return taskTopicAttribute?.TaskTopic;
        }

        [NotNull]
        private static List<Type> GetAllTypesToSearchForAttributes([CanBeNull] Type type)
        {
            if (type == null)
                return new List<Type>();
            return new[] {type}
                .Union(type.GetInterfaces())
                .Union(GetAllTypesToSearchForAttributes(type.BaseType))
                .Distinct()
                .ToList();
        }

        [NotNull]
        private static IEnumerable<TAttribute> GetAttributesForType<TAttribute>([NotNull] Type type)
        {
            return type.GetCustomAttributes(typeof(TAttribute), true).Cast<TAttribute>();
        }
    }
}