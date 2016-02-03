using System;

using SKBKontur.Catalogue.Objects;

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
            var newNowTicks = Math.Max(ticksHolder.GetMaxTicks(globalTicksName) + 1, Timestamp.Now.Ticks);
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

        private readonly ITicksHolder ticksHolder;
        private const string globalTicksName = "GlobalTicks2";
    }
}