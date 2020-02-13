using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RtqTaskNameAttribute : Attribute
    {
        public RtqTaskNameAttribute([NotNull] string taskName)
        {
            TaskName = taskName;
        }

        [NotNull]
        public string TaskName { get; }
    }
}