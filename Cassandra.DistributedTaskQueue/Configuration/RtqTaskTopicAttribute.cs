using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
    public sealed class RtqTaskTopicAttribute : Attribute
    {
        public RtqTaskTopicAttribute([NotNull] string taskTopic)
        {
            TaskTopic = taskTopic;
        }

        [NotNull]
        public string TaskTopic { get; }
    }
}