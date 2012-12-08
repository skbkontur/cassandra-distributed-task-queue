using System;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

namespace FunctionalTests.EventsEnumeratorTests
{
    public class SimpleEventsEnumerableTest : EventsEnumerableTestBase
    {
        [Test]
        public void SimpleStraightOrderTest()
        {
            TestBase(1, false);
        }

        [Test]
        public void SimpleReverseOrderTest()
        {
            TestBase(1, true);
        }

        [Test]
        public void SimpleMultiLineStraightOrderTets()
        {
            TestBase(TimeSpan.FromMinutes(1).Ticks, false);
        }

        [Test]
        public void SimpleMultiLineReverseOrderTest()
        {
            TestBase(TimeSpan.FromMinutes(1).Ticks, true);
        }

        private void TestBase(long step, bool reverseOrder)
        {
            var metas = GenerateMetas(21);

            for(int i = 0; i <= 100; i++)
            {
                if(i % 10 == 0)
                    Console.WriteLine(i);
                foreach(var t in metas)
                {
                    t.MinimalStartTicks += step;
                    t.State = i % 2 == 0 ? TaskState.Finished : TaskState.New;
                    handleTasksMetaStorage.AddMeta(t);
                }
            }
            var expected = (reverseOrder ? metas.Select(x => x.Id).Reverse() : metas.Select(x => x.Id)).ToArray();
            var getEnumerable = GetEnumerable(metas[0].MinimalStartTicks + step * 103 + expected.Length, reverseOrder, TaskState.Finished);
            var y = getEnumerable.ToArray();
            Assert.AreEqual(expected.Length, y.Length);
            for(int i = 0; i < y.Length; i++)
                Assert.AreEqual(expected[i], y[i]);

            getEnumerable = GetEnumerable(metas[0].MinimalStartTicks + step * 103 + expected.Length, reverseOrder, TaskState.New);
            Assert.AreEqual(0, getEnumerable.ToArray().Length);
        }
    }
}