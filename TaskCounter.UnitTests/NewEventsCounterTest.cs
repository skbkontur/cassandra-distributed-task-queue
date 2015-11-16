using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation.NewEventsCounters;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.UnitTests
{
    public class NewEventsCounterTest
    {
        [Test]
        public void TestSimple()
        {
            var counter = new NewTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80)), 100);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 80)), 130);
            CheckCount(counter, 0);
        }

        [Test]
        public void TestSnapshot()
        {
            var counter = new NewTasksCounter(10);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("b", true, 70)), 100);
            CheckCount(counter, 2);

            var newEventsCounterSnapshot = counter.GetSnapshot(100);
            counter.Reset();
            CheckCount(counter, 0);

            Assert.AreEqual(2, newEventsCounterSnapshot.Tasks.Length);

            counter.LoadSnapshot(newEventsCounterSnapshot);

            CheckCount(counter, 2);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 80)), 150);

            CheckCount(counter, 1);
        }

        [Test]
        public void Test11()
        {
            var counter = new NewTasksCounter(10);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("c", true, 99)), 100);
            CheckCount(counter, 1);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("c", false, 99), CreateMeta("d", true, 100)), 101);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", false, 100)), 120);
            CheckCount(counter, 2);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("d", false, 100)), 130);
            CheckCount(counter, 1);
        }

        [Test]
        public void Test2()
        {
            var counter = new NewTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("b", false, 80), CreateMeta("c", true, 99)), 100);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("d", true, 110)), 130);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("e", true, 110)), 130);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", true, 121)), 130);
            CheckCount(counter, 3);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("d", false, 110)), 160);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", false, 1210)), 160);
            CheckCount(counter, 2);
        }

        private static void CheckCount(NewTasksCounter counter, int value)
        {
            Assert.AreEqual(value, counter.GetValue());
        }

        private static TaskMetaInformation[] CreateMetas(params TaskMetaInformation[] metas)
        {
            return metas;
        }

        private static TaskMetaInformation CreateMeta(string id, bool isNew, long mst)
        {
            return new TaskMetaInformation()
                {
                    Id = id,
                    State = isNew ? TaskState.New : TaskState.Finished,
                    MinimalStartTicks = mst
                };
        }
    }
}