using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace FunctionalTests.RepositoriesTests
{
    public class GlobalTimeTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            globalTime = Container.Get<IGlobalTime>();
        }

        [Test]
        public void UpdateNowTicks()
        {
            var lastTicks = 0L;
            for(var i = 0; i < iterations; i++)
            {
                var nowTicks = globalTime.UpdateNowTicks();
                Assert.True(lastTicks < nowTicks);
                lastTicks = nowTicks;
            }
        }

        private IGlobalTime globalTime;
        private const int iterations = 1000;
    }
}