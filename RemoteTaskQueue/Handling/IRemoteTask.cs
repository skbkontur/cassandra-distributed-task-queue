using System;

using JetBrains.Annotations;

namespace RemoteQueue.Handling
{
    public interface IRemoteTask
    {
        [NotNull]
        string Id { get; }

        [NotNull]
        string Queue();

        [NotNull]
        string Queue(TimeSpan delay);
    }
}