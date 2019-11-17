using System;

using SkbKontur.Cassandra.TimeBasedUuid;

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
            var newNowTicks = Math.Max(ticksHolder.GetMaxTicks(globalTicksName) + PreciseTimestampGenerator.TicksPerMicrosecond, Timestamp.Now.Ticks);
            ticksHolder.UpdateMaxTicks(globalTicksName, newNowTicks);
            return newNowTicks;
        }

        public long GetNowTicks()
        {
            return Math.Max(ticksHolder.GetMaxTicks(globalTicksName), Timestamp.Now.Ticks);
        }

        public void ResetInMemoryState()
        {
            ticksHolder.ResetInMemoryState();
        }

        private const string globalTicksName = "GlobalTicks2";
        private readonly ITicksHolder ticksHolder;
    }
}