using System;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

using Rhino.Mocks;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;
using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.UnitTests
{
    public class CounterControllerTest
    {
        [SetUp]
        public void SetUp()
        {
            mockRepository = new MockRepository();

            eventLogRepository = GetMock<IEventLogRepository>();
            metaCachedReader = GetMock<IMetaCachedReader>();
            compositeCounter = GetMock<ICompositeCounter>();
            globalTime = GetMock<IGlobalTime>();
            snapshotStorage = GetMock<ICounterControllerSnapshotStorage>();

            eventLogRepository.Expect(m => m.UnstableZoneLength).Return(TimeSpan.FromTicks(unstableTicks));
            maxBatch = 3;

            controller = new CounterController(eventLogRepository, metaCachedReader, compositeCounter, globalTime, snapshotStorage,
                                               maxHistoryDepthTicks, maxBatch, 1000, maxSnapshotLength);
        }

        [Test]
        public void TestGetCountNotInitialized()
        {
            compositeCounter.Expect(c => c.GetTotalCount()).Return(new TaskCount {Count = 1, UpdateTicks = 123, StartTicks = 348934});
            var totalCount = controller.GetTotalCount();
            totalCount.ShouldBeEquivalentTo(new TaskCount
                {
                    Count = 1,
                    UpdateTicks = 123,
                    StartTicks = 0
                });
        }

        [Test]
        public void TestSimple()
        {
            snapshotStorage.Expect(s => s.ReadSnapshotOrNull()).Return(null);
            compositeCounter.Expect(c => c.Reset());
            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(null);
            DoTest(400 - maxHistoryDepthTicks, 400, null, new[] {GetEvent("a", 301)}, new[] {new[] {GetMeta("a", 10)}});

            var cs0 = new CompositeCounterSnapshot() {TotalSnapshot = new ProcessedTasksCounter.CounterSnapshot(null, 12123, 2)};
            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(cs0);
            var counterControllerSnapshot = new CounterControllerSnapshot
                {
                    CounterSnapshot = cs0,
                    CountollerTicks = 401
                };
            snapshotStorage.Expect(s => s.SaveSnapshot(ARG.EqualsTo(counterControllerSnapshot)));

            DoTest(400, 500, null, new[] {GetEvent("b", 401)}, new[] {new TaskMetaInformation[] {null}});
        }

        [Test]
        public void TestLoadState()
        {
            var cs0 = new CompositeCounterSnapshot() {TotalSnapshot = new ProcessedTasksCounter.CounterSnapshot(null, 12123, 2)};
            snapshotStorage.Expect(s => s.ReadSnapshotOrNull()).Return(new CounterControllerSnapshot()
                {
                    CounterSnapshot = cs0,
                    CountollerTicks = 100
                });
            compositeCounter.Expect(c => c.LoadSnapshot(cs0));
            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(null);
            DoTest(100, 400, null, new[] {GetEvent("a", 301)}, new[] {new[] {GetMeta("a", 10)}});
        }

        [Test]
        public void TestUnprocessed()
        {
            snapshotStorage.Expect(s => s.ReadSnapshotOrNull()).Return(null);
            compositeCounter.Expect(c => c.Reset());
            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(null);
            DoTest(310 - maxHistoryDepthTicks, 310, null, new[] {GetEvent("a", 301), GetEvent("b", 302)}, new[] {new[] {GetMeta("a", 301), null}});

            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(null);
            DoTest(310, 320, new[] {GetEvent("b", 302)}, new[] {GetEvent("c", 311)}, new[] {new[] {GetMeta("b", 303), GetMeta("c", 311)}});
        }

        [Test]
        public void TestUnprocessedBug()
        {
            snapshotStorage.Expect(s => s.ReadSnapshotOrNull()).Return(null);
            compositeCounter.Expect(c => c.Reset());
            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(null);
            DoTest(310 - maxHistoryDepthTicks, 310, null, new[] {GetEvent("a", 301), GetEvent("b", 302), GetEvent("b", 303)}, new[] {new[] {GetMeta("a", 301), null, null}});

            compositeCounter.Expect(m => m.GetSnapshotOrNull(maxSnapshotLength)).Return(null);
            DoTest(310, 320, new[] {GetEvent("b", 303)}, new[] {GetEvent("c", 311)}, new[] {new[] {GetMeta("b", 303), GetMeta("c", 311)}});
        }

        private static TaskMetaUpdatedEvent GetEvent(string taskId, long ticks)
        {
            return new TaskMetaUpdatedEvent() {TaskId = taskId, Ticks = ticks};
        }

        private static TaskMetaInformation GetMeta(string taskId, long lastModificationTicks)
        {
            return new TaskMetaInformation() {Id = taskId, LastModificationTicks = lastModificationTicks};
        }

        private void DoTest(long lastTicks, long nowTicks, TaskMetaUpdatedEvent[] unprocessed, TaskMetaUpdatedEvent[] events, TaskMetaInformation[][] metaBatches)
        {
            ExpectNow(nowTicks);
            var allEvents = unprocessed == null ? events : unprocessed.Concat(events).ToArray();
            eventLogRepository.Expect(m => m.GetEvents(lastTicks - unstableTicks, maxBatch)).Return(events);
            var count = 0;
            Assert.AreEqual(metaBatches.Sum(x => x.Length), allEvents.Length, "Bad batch");
            foreach(var metaBatch in metaBatches)
            {
                var eventsCopy = new TaskMetaUpdatedEvent[metaBatch.Length];
                Array.Copy(allEvents, count, eventsCopy, 0, metaBatch.Length);
                count += metaBatch.Length;
                metaCachedReader.Expect(m => m.ReadActualMetasQuiet(eventsCopy, nowTicks)).Return(metaBatch);
                var batch2 = metaBatch.ToArray();
                compositeCounter.Expect(c => c.ProcessMetas(ARG.EqualsTo(batch2.Where(x => x != null).ToArray()), ARG.EqualsTo(nowTicks)));
            }

            if(allEvents.Length == 0)
                compositeCounter.Expect(c => c.ProcessMetas(ARG.EqualsTo(new TaskMetaInformation[0]), ARG.EqualsTo(nowTicks)));

            controller.ProcessNewEvents();

            VerifyAll();
        }

        private void ExpectNow(long nowTicks)
        {
            globalTime.Expect(m => m.GetNowTicks()).Return(nowTicks);
        }

        private T GetMock<T>()
        {
            var mock = mockRepository.StrictMock<T>();
            mock.Replay();
            return mock;
        }

        private void VerifyAll()
        {
            mockRepository.VerifyAll();
            mockRepository.BackToRecordAll(BackToRecordOptions.None);
            mockRepository.ReplayAll();
        }

        private const int maxSnapshotLength = 11;
        private const int unstableTicks = 10;

        private const int maxHistoryDepthTicks = 200;

        private CounterController controller;
        private IEventLogRepository eventLogRepository;
        private IMetaCachedReader metaCachedReader;
        private ICompositeCounter compositeCounter;
        private IGlobalTime globalTime;
        private ICounterControllerSnapshotStorage snapshotStorage;
        private int maxBatch;
        private MockRepository mockRepository;
    }
}