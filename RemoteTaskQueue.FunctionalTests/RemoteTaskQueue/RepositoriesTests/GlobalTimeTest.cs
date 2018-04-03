using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;
using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [EdiTestSuite("TicksHolderTests")]
    public class GlobalTimeTest : ITestRtqCassandraWithTickHolderTestSuite
    {
        [Test]
        public void UpdateNowTicks()
        {
            var lastTicks = 0L;
            for(var i = 0; i < 1000; i++)
            {
                var nowTicks = globalTime.UpdateNowTicks();
                Assert.True(lastTicks <= nowTicks + PreciseTimestampGenerator.TicksPerMicrosecond);
                lastTicks = nowTicks;
            }
        }

        [Injected]
        private readonly GlobalTime globalTime;
    }
}