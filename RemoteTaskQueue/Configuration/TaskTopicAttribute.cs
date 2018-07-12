using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace RemoteQueue.Configuration
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class TaskTopicAttribute : Attribute
    {
        public TaskTopicAttribute([NotNull] string taskTopic)
        {
            TaskTopic = taskTopic;
        }

        [NotNull]
        public string TaskTopic { get; private set; }
    }
}