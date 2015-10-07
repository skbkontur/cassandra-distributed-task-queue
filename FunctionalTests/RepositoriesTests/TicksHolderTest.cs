using System;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace FunctionalTests.RepositoriesTests
{
    public class TicksHolderTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            ticksHolder = Container.Get<ITicksHolder>();
        }

        [Test]
        public void UpdateMaxTicks()
        {
            var ticks = DateTime.UtcNow.Ticks;
            Assert.AreEqual(ticks, ticksHolder.UpdateMaxTicks("r", ticks));
            Assert.AreEqual(ticks + 2, ticksHolder.UpdateMaxTicks("r1", ticks + 2));
            Assert.AreEqual(ticks + 2, ticksHolder.UpdateMaxTicks("r1", ticks + 1));
            Assert.AreEqual(ticks, ticksHolder.GetMaxTicks("r"));
            Assert.AreEqual(ticks + 2, ticksHolder.UpdateMaxTicks("r", ticks + 2));
            Assert.AreEqual(ticks + 2, ticksHolder.GetMaxTicks("r"));
        }

        [Test]
        public void UpdateMinTicks()
        {
            var ticks = DateTime.UtcNow.Ticks;
            Assert.AreEqual(ticks, UpdateMinTicks("r", ticks));
            Assert.AreEqual(ticks - 2, UpdateMinTicks("r1", ticks - 2));
            Assert.AreEqual(ticks - 2, UpdateMinTicks("r1", ticks - 1));
            Assert.AreEqual(ticks, ticksHolder.GetMinTicks("r"));
            Assert.AreEqual(ticks - 2, UpdateMinTicks("r", ticks - 2));
            Assert.AreEqual(ticks - 2, ticksHolder.GetMinTicks("r"));
        }

        private long UpdateMinTicks(string name, long ticks)
        {
            ticksHolder.UpdateMinTicks(name, ticks);
            return ticksHolder.GetMinTicks(name);
        }

        private ITicksHolder ticksHolder;
    }
}