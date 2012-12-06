using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.EventIndexes;

namespace FunctionalTests.RepositoriesTests
{
    public class TaskMetaEventColumnInfoIndexTest : FunctionalTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            columnInfoIndex = Container.Get<ITaskMetaEventColumnInfoIndex>();
        }

        [Test]
        public void GetPreviousTaskEventsTest()
        {
            const string taskId = "a";
            var columnInfo = new ColumnInfo {RowKey = "b", ColumnName = "01111111111111111111_c"};
            var columnInfo2 = new ColumnInfo {RowKey = "b", ColumnName = "01111111111111111113_c"};
            columnInfoIndex.AddTaskEventInfo(taskId, columnInfo);
            columnInfoIndex.AddTaskEventInfo(taskId, columnInfo2);
            ColumnInfo[] infos1 = columnInfoIndex.GetPreviousTaskEvents(taskId, columnInfo);
            Assert.AreEqual(0, infos1.Length);
            ColumnInfo[] infos2 = columnInfoIndex.GetPreviousTaskEvents(taskId, columnInfo2);
            Assert.AreEqual(1, infos2.Length);
            infos2[0].AssertEqualsTo(columnInfo);
        }

        [Test]
        public void GetPreviousTaskEventsWithMultipleTasksTest()
        {
            const string firstTaskId = "a";
            const string secondTaskId = "b";
            var columnInfo = new ColumnInfo {RowKey = "b", ColumnName = "01111111111111111111_c"};
            var columnInfo2 = new ColumnInfo {RowKey = "e", ColumnName = "01111111111111111113_c"};
            columnInfoIndex.AddTaskEventInfo(firstTaskId, columnInfo);
            columnInfoIndex.AddTaskEventInfo(secondTaskId, columnInfo2);
            ColumnInfo[] infos1 = columnInfoIndex.GetPreviousTaskEvents(firstTaskId, columnInfo);
            Assert.AreEqual(0, infos1.Length);
            ColumnInfo[] infos2 = columnInfoIndex.GetPreviousTaskEvents(secondTaskId, columnInfo2);
            Assert.AreEqual(0, infos2.Length);
        }

        private ITaskMetaEventColumnInfoIndex columnInfoIndex;
    }
}