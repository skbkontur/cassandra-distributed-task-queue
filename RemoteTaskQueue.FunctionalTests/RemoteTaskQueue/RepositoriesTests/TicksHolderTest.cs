using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GroboContainer.NUnitExtensions;

using MoreLinq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.RepositoriesTests
{
    [GroboTestSuite("TicksHolderTests")]
    public class TicksHolderTest : ITestRtqCassandraWithTickHolderTestSuite
    {
        [Test]
        public void UpdateMaxTicks()
        {
            var ticks = Timestamp.Now.Ticks;
            Assert.AreEqual(ticks, UpdateMaxTicks("r", ticks));
            Assert.AreEqual(ticks + 2, UpdateMaxTicks("r1", ticks + 2));
            Assert.AreEqual(ticks + 2, UpdateMaxTicks("r1", ticks + 1));
            Assert.AreEqual(ticks, ticksHolder.GetMaxTicks("r"));
            Assert.AreEqual(ticks + 2, UpdateMaxTicks("r", ticks + 2));
            Assert.AreEqual(ticks + 2, ticksHolder.GetMaxTicks("r"));
        }

        [Test]
        public void UpdateMinTicks()
        {
            var ticks = Timestamp.Now.Ticks;
            Assert.AreEqual(ticks, UpdateMinTicks("r", ticks));
            Assert.AreEqual(ticks - 2, UpdateMinTicks("r1", ticks - 2));
            Assert.AreEqual(ticks - 2, UpdateMinTicks("r1", ticks - 1));
            Assert.AreEqual(ticks, ticksHolder.GetMinTicks("r"));
            Assert.AreEqual(ticks - 2, UpdateMinTicks("r", ticks - 2));
            Assert.AreEqual(ticks - 2, ticksHolder.GetMinTicks("r"));
        }

        [Test]
        public void ConcurrentUpdates()
        {
            var key = Guid.NewGuid().ToString();
            const int threadsCount = 4;
            const int countPerThread = 1000 * 1000;
            const int valuesCount = threadsCount * countPerThread;
            var rng = new Random(Guid.NewGuid().GetHashCode());
            var values = Enumerable.Range(0, valuesCount).Select(x => rng.Next(valuesCount)).ToList();
            var valuesByThread = values.Batch(countPerThread, Enumerable.ToArray).ToArray();
            var threads = new List<Thread>();
            var startSignal = new ManualResetEvent(false);
            for (var i = 0; i < threadsCount; i++)
            {
                var threadIndex = i;
                var thread = new Thread(() =>
                    {
                        startSignal.WaitOne();
                        foreach (var value in valuesByThread[threadIndex])
                        {
                            ticksHolder.UpdateMinTicks(key, value);
                            ticksHolder.UpdateMaxTicks(key, value);
                        }
                    });
                thread.Start();
                threads.Add(thread);
            }
            startSignal.Set();
            threads.ForEach(thread => thread.Join());
            Assert.That(ticksHolder.GetMinTicks(key), Is.EqualTo(values.Min()));
            Assert.That(ticksHolder.GetMaxTicks(key), Is.EqualTo(values.Max()));
        }

        private long UpdateMaxTicks(string name, long ticks)
        {
            ticksHolder.UpdateMaxTicks(name, ticks);
            return ticksHolder.GetMaxTicks(name);
        }

        private long UpdateMinTicks(string name, long ticks)
        {
            ticksHolder.UpdateMinTicks(name, ticks);
            return ticksHolder.GetMinTicks(name);
        }

        [Injected]
        private readonly TicksHolder ticksHolder;
    }
}