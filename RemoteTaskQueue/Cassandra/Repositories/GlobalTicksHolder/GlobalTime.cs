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
            var newNowTicks = Math.Max(ticksHolder.GetMaxTicks(globalTicksName) + 1, DateTime.UtcNow.Ticks);
            ticksHolder.UpdateMaxTicks(globalTicksName, newNowTicks);
            return newNowTicks;
        }

        public long GetNowTicks()
        {
            return Math.Max(ticksHolder.GetMaxTicks(globalTicksName), DateTime.UtcNow.Ticks);
        }

        private readonly ITicksHolder ticksHolder;
        private const string globalTicksName = "GlobalTicks2";
    }
}