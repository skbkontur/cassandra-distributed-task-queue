using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using RemoteTaskQueue.TaskCounter.Implementation.OldWaitingTasksCounters;

namespace RemoteTaskQueue.UnitTests.TaskCounter
{
    public class OldWaitingTasksCounterTest
    {
        [Test]
        public void TestSimple()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80)), 100);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 80)), 130);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestOrderBug1()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 95), CreateMeta("a", false, 95)), 100);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 100), CreateMeta("a", false, 100)), 120);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(), 500);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestOrderBug3()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("a", false, 80), CreateMeta("a", true, 95)), 100);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 95)), 110);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(), 500);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestOrderBug2()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 95)), 100);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 95)), 101);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(), 150);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestRerunTask1()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80)), 100);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 80)), 130);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 145)), 160);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 145)), 200);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 215)), 220);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 215)), 230);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestRerunTask2()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 95)), 100);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 105)), 120);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 129)), 140);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 155)), 200);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestRerunTask3()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 95)), 100);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 200)), 120);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", false, 100)), 220);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 200)), 230);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestRerunTask4()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 95)), 100);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 200)), 120);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 200)), 220);
            CheckCount(counter, 0);

            CheckAllProcessed(counter);
        }

        [Test]
        public void TestSnapshot()
        {
            var counter = new OldWaitingTasksCounter(10);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("b", true, 70), CreateMeta("z", true, 99)), 100);
            CheckCount(counter, 2);

            var newEventsCounterSnapshot = counter.GetSnapshot(100);
            counter.Reset();
            CheckCount(counter, 0);

            counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            Assert.AreEqual(2, newEventsCounterSnapshot.Tasks.Length);

            counter.LoadSnapshot(newEventsCounterSnapshot);

            CheckCount(counter, 2);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", false, 80)), 150);

            CheckCount(counter, 2);

            CheckAllProcessed(counter);
        }

        [Test]
        public void Test11()
        {
            var counter = new OldWaitingTasksCounter(10);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("c", true, 99)), 100);
            CheckCount(counter, 1);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("c", false, 99), CreateMeta("d", true, 100)), 101);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", false, 100)), 120);
            CheckCount(counter, 2);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("d", false, 100)), 130);
            CheckCount(counter, 1);

            CheckAllProcessed(counter);
        }

        [Test]
        public void Test2()
        {
            var counter = new OldWaitingTasksCounter(10);
            CheckCount(counter, 0);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", true, 80), CreateMeta("b", false, 80), CreateMeta("c", true, 99)), 100);
            CheckCount(counter, 1);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("d", true, 110)), 130);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("e", true, 110)), 130);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", true, 121)), 130);
            CheckCount(counter, 4);

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("d", false, 110)), 160);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("z", false, 121)), 160);
            CheckCount(counter, 3);

            CheckAllProcessed(counter);
        }

        private static void CheckAllProcessed(OldWaitingTasksCounter c)
        {
            Assert.AreEqual(0, c.GetStatus().NotCountedNewTasksCount, "not all tasks processed");
        }

        private static void CheckCount(OldWaitingTasksCounter counter, int value)
        {
            Assert.AreEqual(value, counter.GetValue());
        }

        private static TaskMetaInformation[] CreateMetas(params TaskMetaInformation[] metas)
        {
            return metas;
        }

        private static TaskMetaInformation CreateMeta(string id, bool isNew, long mst)
        {
            return new TaskMetaInformation("TaskName", id)
                {
                    State = isNew ? TaskState.New : TaskState.Finished,
                    MinimalStartTicks = mst
                };
        }
    }
}