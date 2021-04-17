using System;

namespace RemoteTaskQueue.FunctionalTests
{
    public static class TimeSpanExtensions
    {
        public static TimeSpan Multiply(this TimeSpan time, double multiplier) => TimeSpan.FromTicks((long)(time.Ticks * multiplier));
    }
}