using System;
using System.Diagnostics.CodeAnalysis;

using JetBrains.Annotations;

namespace RemoteQueue.Configuration
{
    [SuppressMessage("ReSharper", "RedundantAttributeUsageProperty")]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TaskNameAttribute : Attribute
    {
        public TaskNameAttribute([NotNull] string taskName)
        {
            TaskName = taskName;
        }

        [NotNull]
        public string TaskName { get; private set; }
    }
}