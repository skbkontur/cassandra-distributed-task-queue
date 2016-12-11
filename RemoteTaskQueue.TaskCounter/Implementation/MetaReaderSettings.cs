using System;

namespace RemoteTaskQueue.TaskCounter.Implementation
{
    public class MetaReaderSettings
    {
        public static readonly TimeSpan CacheInterval = TimeSpan.FromMinutes(10);
    }
}