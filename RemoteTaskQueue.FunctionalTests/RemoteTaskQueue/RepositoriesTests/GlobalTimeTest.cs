using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    public class GlobalTimeTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            globalTime = Container.Get<GlobalTime>();
        }

        [Test]
        public void UpdateNowTicks()
        {
            var lastTicks = 0L;
            for(var i = 0; i < iterations; i++)
            {
                var nowTicks = globalTime.UpdateNowTicks();
                Assert.True(lastTicks <= nowTicks + PreciseTimestampGenerator.TicksPerMicrosecond);
                lastTicks = nowTicks;
            }
        }

        private GlobalTime globalTime;
        private const int iterations = 1000;
    }
}