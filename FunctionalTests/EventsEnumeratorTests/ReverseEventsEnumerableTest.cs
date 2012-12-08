using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;

namespace FunctionalTests.EventsEnumeratorTests
{
    public class ReverseEventsEnumerableTest : EventsEnumerableTestBase
    {
        [Test]
        public void Test()
        {
            var states = new[] {TaskState.New, TaskState.InProcess, TaskState.Finished};
            var metas = GenerateMetas(11);

            foreach(var t in metas)
            {
                t.State = states[t.MinimalStartTicks % 3];
                handleTasksMetaStorage.AddMeta(t);
            }

            var maxTicks = metas[10].MinimalStartTicks;
            var minTicks = metas[0].MinimalStartTicks;

            const bool  reverseOrder = true;

            var getEnumerable = GetEnumerable(maxTicks - 1, reverseOrder, TaskState.New, TaskState.InProcess, TaskState.Finished);
            Assert.AreEqual(10, getEnumerable.ToArray().Length);
            getEnumerable = GetEnumerable(maxTicks, reverseOrder, TaskState.New, TaskState.InProcess, TaskState.Finished);
            Assert.AreEqual(11, getEnumerable.ToArray().Length);
            getEnumerable = GetEnumerable(maxTicks + 1, reverseOrder, TaskState.New, TaskState.InProcess, TaskState.Finished);
            Assert.AreEqual(11, getEnumerable.ToArray().Length);

            getEnumerable = GetEnumerable(minTicks, reverseOrder, TaskState.New, TaskState.InProcess, TaskState.Finished);
            Assert.AreEqual(1, getEnumerable.ToArray().Length);
            getEnumerable = GetEnumerable(minTicks - 1, reverseOrder, TaskState.New, TaskState.InProcess, TaskState.Finished);
            Assert.AreEqual(0, getEnumerable.ToArray().Length);
        }
    }
}