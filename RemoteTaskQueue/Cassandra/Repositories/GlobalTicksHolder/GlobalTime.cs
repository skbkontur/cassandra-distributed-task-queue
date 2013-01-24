using System;

namespace RemoteQueue.Cassandra.Repositories.GlobalTicksHolder
{
    public class GlobalTime : IGlobalTime
    {
        public GlobalTime(ITicksHolder ticksHolder)
        {
            this.ticksHolder = ticksHolder;
        }

        public long UpdateNowTicks()
        {
            var actualTicks = Math.Max(DateTime.UtcNow.Ticks, ticksHolder.GetMaxTicks(globalTicksName) + 1);
            ticksHolder.UpdateMaxTicks(globalTicksName, actualTicks);
            return actualTicks;
        }

        public long GetNowTicks()
        {
            return Math.Max(ticksHolder.GetMaxTicks(globalTicksName), DateTime.UtcNow.Ticks);
        }

        private readonly ITicksHolder ticksHolder;
        private const string globalTicksName = "GlobalTicks";
    }
}