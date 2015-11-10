using FluentAssertions;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation;
using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;
using SKBKontur.Catalogue.TestCore;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.UnitTests
{
    public class ProcessedTasksCounterTest
    {
        [Test]
        public void TestSimple()
        {
            var counter = new ProcessedTasksCounter();
            CheckCounts(counter, 0, 0, GetCounts(0, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 100)), 100);
            CheckCounts(counter, 1, 100, GetCounts(1, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Finished, 100)), 101);
            CheckCounts(counter, 0, 100, GetCounts(0, 0));
        }

        [Test]
        public void TestTask1()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 1)), 0);
            CheckCounts(counter, 1, 1, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 2)), 0);
            CheckCounts(counter, 1, 2, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Canceled, 10)), 0);
            CheckCounts(counter, 0, 10, GetCounts(0, 0));
        }

        [Test]
        public void TestTaskLife()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 1)), 0);
            CheckCounts(counter, 1, 1, GetCounts(1, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 2)), 0);
            CheckCounts(counter, 1, 2, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.WaitingForRerun, 3)), 0);
            CheckCounts(counter, 1, 3, GetCounts(0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.WaitingForRerunAfterError, 4)), 0);
            CheckCounts(counter, 1, 4, GetCounts(0, 0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.WaitingForRerunAfterError, 5)), 0);
            CheckCounts(counter, 1, 5, GetCounts(0, 0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Canceled, 10)), 0);
            CheckCounts(counter, 0, 10, GetCounts(0, 0));
        }

        [Test]
        public void TestTask2()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Fatal, 1)), 0);
            CheckCounts(counter, 0, 1, GetCounts(0, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Fatal, 2)), 0);
            CheckCounts(counter, 0, 2, GetCounts(0, 0));
        }

        [Test]
        public void TestReset()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 10)), 11);
            CheckCounts(counter, 1, 10, GetCounts(1, 0));

            counter.Reset();
            CheckCounts(counter, 0, 0, GetCounts(0, 0));
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Fatal, 11)), 12);

            CheckCounts(counter, 0, 11, GetCounts(0, 0));
        }

        [Test]
        public void TestSnapshots()
        {
            var counter = new ProcessedTasksCounter();
            var emptySnapshot = counter.GetSnapshotOrNull(2);
            counter.LoadSnapshot(emptySnapshot);
            CheckCounts(counter, 0, 0, GetCounts(0,0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 100)), 0);
            CheckCounts(counter, 1, 100, GetCounts(1, 0));
            var s1 = counter.GetSnapshotOrNull(2);

            counter.LoadSnapshot(emptySnapshot);
            CheckCounts(counter, 0, 0, GetCounts(0, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 100)), 0);
            CheckCounts(counter, 1, 100, GetCounts(1, 0));

            var s2 = counter.GetSnapshotOrNull(2);
            s2.ShouldBeEquivalentTo(s1);

            counter.LoadSnapshot(s1);
            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Fatal, 106)), 0);
            CheckCounts(counter, 0, 106, GetCounts(0, 0));
        }

        [Test]
        public void TestTask3()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 100)), 0);
            CheckCounts(counter, 1, 100, GetCounts(1, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 101)), 0);
            CheckCounts(counter, 1, 101, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.WaitingForRerun, 102)), 0);
            CheckCounts(counter, 1, 102, GetCounts(0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 103)), 0);
            CheckCounts(counter, 1, 103, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.WaitingForRerunAfterError, 104)), 0);
            CheckCounts(counter, 1, 104, GetCounts(0, 0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 105)), 0);
            CheckCounts(counter, 1, 105, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Fatal, 106)), 0);
            CheckCounts(counter, 0, 106, GetCounts(0, 0));
        }

        [Test]
        public void TestTask4()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 1)), 0);
            CheckCounts(counter, 1, 1, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.InProcess, 1)), 0);
            CheckCounts(counter, 1, 1, GetCounts(0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Canceled, 10)), 9);
            CheckCounts(counter, 0, 10, GetCounts(0, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Canceled, 10)), 11);
            CheckCounts(counter, 0, 10, GetCounts(0, 0));
        }

        [Test]
        public void TestManyTasks1()
        {
            var counter = new ProcessedTasksCounter();

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.New, 100)), 0);
            CheckCounts(counter, 1, 100, GetCounts(1, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("b", TaskState.Finished, 101)), 0);
            CheckCounts(counter, 1, 101, GetCounts(1, 0));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("c", TaskState.InProcess, 102)), 0);
            CheckCounts(counter, 2, 102, GetCounts(1, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("c", TaskState.WaitingForRerunAfterError, 103)), 0);
            CheckCounts(counter, 2, 103, GetCounts(1, 0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("a", TaskState.Finished, 104)), 0);
            CheckCounts(counter, 1, 104, GetCounts(0, 0, 0, 0, 1));

            counter.NewMetainformationAvailable(CreateMetas(CreateMeta("c", TaskState.Finished, 105)), 0);
            CheckCounts(counter, 0, 105, GetCounts(0, 0));
        }

        private static int[] GetCounts(int @new, int inProcess, int unk = 0, int waitingForRerun = 0, int waitingForRerunAfterError = 0)
        {
            return new[] {unk, @new, waitingForRerun, waitingForRerunAfterError, 0, inProcess, 0, 0};
        }

        private static void CheckCounts(ProcessedTasksCounter counter, int count, long ticks, int[] counts)
        {
            counter.GetCount().AssertEqualsTo(new TaskCount {Count = count, UpdateTicks = ticks, Counts = counts});
            counter.GetNotFinishedTasksCount().ShouldBeEquivalentTo(count); //note debug check
        }

        private static TaskMetaInformation[] CreateMetas(params TaskMetaInformation[] metas)
        {
            return metas;
        }

        private static TaskMetaInformation CreateMeta(string id, TaskState s, long? updateTicks)
        {
            return new TaskMetaInformation()
                {
                    Id = id,
                    State = s,
                    LastModificationTicks = updateTicks
                };
        }
    }
}