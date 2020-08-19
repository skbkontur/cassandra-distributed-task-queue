using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Scheduling
{
    public static class DateTimeMath
    {
        public static TimeSpan Max(TimeSpan first, TimeSpan second)
        {
            return first >= second ? first : second;
        }
    }
}