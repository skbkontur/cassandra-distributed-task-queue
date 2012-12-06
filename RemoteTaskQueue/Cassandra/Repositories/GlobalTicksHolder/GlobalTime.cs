using System;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public class GlobalTime : IGlobalTime
    {
        public GlobalTime(ITicksHolder ticksHolder)
        {
            this.ticksHolder = ticksHolder;
        }

        public long GetNowTicks()
        {
            var actualTicks = Math.Max(DateTime.UtcNow.Ticks, ticksHolder.GetMaxTicks(globalTicksName) + 1);
            ticksHolder.UpdateMaxTicks(globalTicksName, actualTicks);
            return actualTicks;
        }

        private readonly ITicksHolder ticksHolder;
        private const string globalTicksName = "GlobalTicks";
    }
}