using System;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using Rhino.Mocks;

using SKBKontur.Catalogue.RemoteTaskQueue.MonitoringServiceCore.Implementation.Counters;
using SKBKontur.Catalogue.TestCore;

namespace RemoteQueue.Tests.MonitoringServiceCore
{
    [TestFixture]
    public class MetaProviderTest : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            eventLog = GetMock<IEventLogRepository>();
            globalTime = GetMock<IGlobalTime>();
            metaStorage = GetMock<IHandleTasksMetaStorage>();

            eventLog.Expect(m => m.UnstableZoneLength).Return(TimeSpan.FromTicks(unstableTicks)).Repeat.Any();
            metaConsumer = GetMock<IMetaConsumer>();
            metaProvider = new MetaProvider(lifeTicks, maxBatch, 0, eventLog, globalTime, metaStorage, new[] {metaConsumer,});
        }

        [Test]
        public void TestStupid()
        {
            DoTest(0,
                   100,
                   new[] {GetEvent("a", 1)},
                   new[] {"a"},
                   new[] {GetMeta("a", 1)},
                   new[] {0});
            DoTest(100 - unstableTicks,
                   200,
                   new[] {GetEvent("b", 101)},
                   new[] {"b"},
                   new[] {GetMeta("b", 102)},
                   new[] {0});
        }

        [Test]
        public void TestRestart()
        {
            DoTest(0,
                   50,
                   new[] {GetEvent("a", 1), GetEvent("b", 2)},
                   new[] {"a", "b"},
                   new TaskMetaInformation[0],
                   new int[0]);

            metaProvider.Restart(100);

            DoTest(100 - unstableTicks,
                   200,
                   new[] {GetEvent("a", 101)},
                   new[] {"a"},
                   new[] {GetMeta("a", 101)},
                   new[] {0});
        }

        [Test]
        public void TestSnapshots()
        {
            var empty = metaProvider.GetSnapshotOrNull(10);
            DoTest(0,
                   15,
                   new[] {GetEvent("a", 1)},
                   new[] {"a"},
                   new[] {GetMeta("a", 1)},
                   new[] {0});
            Assert.AreEqual(0, empty.NotReadEvents.Count);
            Assert.AreEqual(0, empty.ReadEvents.Count);
            Assert.AreEqual(0, empty.LastUpdateTicks);

            var notEmpty = metaProvider.GetSnapshotOrNull(10);

            Assert.AreEqual(0, notEmpty.NotReadEvents.Count);
            Assert.AreEqual(1, notEmpty.ReadEvents.Count);
            Assert.AreEqual(15, notEmpty.LastUpdateTicks);
            Assert.AreEqual(0, notEmpty.StartTicks);

            metaProvider.LoadSnapshot(empty, 0);
            DoTest(0,
                   200,
                   new[] {GetEvent("z", 10)},
                   new[] {"z"},
                   new TaskMetaInformation[] {},
                   new int[] {});

            var s2 = metaProvider.GetSnapshotOrNull(1);

            Assert.AreEqual(1, s2.NotReadEvents.Count);
            Assert.AreEqual(0, empty.ReadEvents.Count);
            Assert.AreEqual(200, s2.LastUpdateTicks);
        }

        [Test]
        public void TestLoadSnapshotTicksLimitation()
        {
            metaProvider = new MetaProvider(lifeTicks, maxBatch, 11, eventLog, globalTime, metaStorage, new[] {metaConsumer,});
            DoTest(11 - unstableTicks,
                   200,
                   new[] {GetEvent("a", 101)},
                   new[] {"a"},
                   new[] {GetMeta("a", 101)},
                   new[] {0});

            //note snapshot not loaded. reset(101)
            metaProvider.LoadSnapshot(new MetaProvider.MetaProviderSnapshot(100, 100, null, null), 101);

            DoTest(101 - unstableTicks,
                   200,
                   new[] {GetEvent("z", 101)},
                   new[] {"z"},
                   new[] {GetMeta("z", 101)},
                   new[] {0});
        }

        [Test]
        public void TestGarbageCollected()
        {
            DoTest(0,
                   100,
                   new[] {GetEvent("a", 10)},
                   new[] {"a"},
                   new[] {GetMeta("a", 9)},
                   new int[0]);
            DoTest(100 - unstableTicks,
                   lifeTicks + 10,
                   new[] {GetEvent("b", 10)},
                   new[] {"a", "b"},
                   new[] {GetMeta("b", 10)},
                   new[] {0});
            DoTest(lifeTicks + 10 - unstableTicks,
                   lifeTicks + 300,
                   new[] {GetEvent("c", 10)},
                   new[] {"c"},
                   new[] {GetMeta("c", 10)},
                   new[] {0});
        }

        [Test]
        public void TestNoEventsBug()
        {
            DoTest(0,
                   100,
                   new TaskMetaUpdatedEvent[0],
                   new string[0],
                   new TaskMetaInformation[] {}, 
                   new int[0]);
        }

        [Test]
        public void TestEventsDoNotReadTwiceInTimeout()
        {
            DoTest(0,
                   5,
                   new[] {GetEvent("a", 1), GetEvent("b", 2)},
                   new[] {"a", "b"},
                   new[] {GetMeta("a", 1)},
                   new[] {0});
            DoTest(0,
                   6,
                   new[] {GetEvent("a", 1), GetEvent("b", 2)},
                   new[] {"b"},
                   new[] {GetMeta("b", 2)},
                   new[] {0});

            DoTest(0,
                   2 * unstableTicks + 1 + 1,
                   new[] {GetEvent("a", 1)},
                   new[] {"a"},
                   new[] {GetMeta("a", 1)},
                   new[] {0});
        }

        [Test]
        public void TestOldObjectsNotProcessed()
        {
            DoTest(0,
                   100,
                   new[] {GetEvent("a", 10)},
                   new[] {"a"},
                   new[] {GetMeta("a", 4)},
                   new int[0]);
            DoTest(100 - unstableTicks,
                   200,
                   new[] {GetEvent("b", 100)},
                   new[] {"a", "b"},
                   new[] {GetMeta("a", 10), GetMeta("b", 99)},
                   new[] {0});
            DoTest(200 - unstableTicks,
                   300,
                   new[] {GetEvent("c", 199)},
                   new[] {"b", "c"},
                   new[] {GetMeta("b", 101), GetMeta("c", 198)},
                   new[] {0});
            DoTest(300 - unstableTicks,
                   400,
                   new[] {GetEvent("c", 300)},
                   new[] {"c"},
                   new[] {GetMeta("c", 300)},
                   new[] {0});
        }

        [Test]
        public void TestObjectReadOnce()
        {
            DoTest(0,
                   100,
                   new[] {GetEvent("a", 1), GetEvent("a", 2), GetEvent("a", 4), GetEvent("b", 10)},
                   new[] {"a", "b"},
                   new[] {GetMeta("a", 4), GetMeta("b", 11)},
                   new[] {0, 1});
        }

        private void DoTest(long lastTicks, long nowTicks, TaskMetaUpdatedEvent[] events, string[] ids, TaskMetaInformation[] readMetas, int[] goodMetas)
        {
            globalTime.Expect(m => m.GetNowTicks()).Return(nowTicks);

            eventLog.Expect(m => m.GetEvents(lastTicks, maxBatch)).Return(events);

            //todo bug. onknown order for .Keys
            metaStorage.Expect(m => m.GetMetas(
                ARG.EqualsTo(ids))).Return(readMetas);

            if(goodMetas != null)
            {
                var metasToUpdate = goodMetas.Select(x => readMetas[x]).ToArray();
                metaConsumer.Expect(c => c.NewMetainformationAvailable(ARG.EqualsTo(metasToUpdate), ARG.EqualsTo(nowTicks)));
            }

            metaProvider.FetchMetas();

            VerifyAll();
        }

        private static TaskMetaUpdatedEvent GetEvent(string taskId, long ticks)
        {
            return new TaskMetaUpdatedEvent() {TaskId = taskId, Ticks = ticks};
        }

        private static TaskMetaInformation GetMeta(string taskId, long lastModificationTicks)
        {
            return new TaskMetaInformation() {Id = taskId, LastModificationTicks = lastModificationTicks};
        }

        private IEventLogRepository eventLog;
        private IGlobalTime globalTime;
        private IHandleTasksMetaStorage metaStorage;
        private MetaProvider metaProvider;
        private IMetaConsumer metaConsumer;
        private const int maxBatch = 4;
        private const int unstableTicks = 10;
        private const long lifeTicks = 1000;
    }
}