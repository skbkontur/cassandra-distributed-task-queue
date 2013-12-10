using System;

using NUnit.Framework;

using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;

using SKBKontur.Catalogue.TestCore;

namespace RemoteQueue.Tests.Cassandra.Repositories.Indexes.StartTicksIndexes
{
    public class TasksCacheTest : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            tasksCache = new TasksCache();
        }

        [Test]
        public void TestPassEmpty()
        {
            CollectionAssert.IsEmpty(tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
        }

        [Test]
        public void TestCacheWorking()
        {
            var element1 = new Tuple<string, ColumnInfo>("taskId", new ColumnInfo
                {
                    ColumnName = "columnName",
                    RowKey = "rowKey"
                });
            CollectionAssert.AreEquivalent(new[] {element1}, tasksCache.PassThroughtCache(new[] {element1}));
            CollectionAssert.AreEquivalent(new[] {element1}, tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
        }

        [Test]
        public void TestUpdateValueInCache()
        {
            var element_v1 = new Tuple<string, ColumnInfo>("taskId", new ColumnInfo
                {
                    ColumnName = "columnName",
                    RowKey = "rowKey"
                });
            var element_v2 = new Tuple<string, ColumnInfo>("taskId", new ColumnInfo
                {
                    ColumnName = "columnName1",
                    RowKey = "rowKey1"
                });
            CollectionAssert.AreEquivalent(new[] {element_v1}, tasksCache.PassThroughtCache(new[] {element_v1}));
            CollectionAssert.AreEquivalent(new[] {element_v1, element_v2}, tasksCache.PassThroughtCache(new[] {element_v2}));
            CollectionAssert.AreEquivalent(new[] {element_v2}, tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
        }

        [Test]
        public void TestRemoving()
        {
            var element1 = new Tuple<string, ColumnInfo>("taskId1", new ColumnInfo
                {
                    ColumnName = "columnName1",
                    RowKey = "rowKey1"
                });
            var element2 = new Tuple<string, ColumnInfo>("taskId2", new ColumnInfo
                {
                    ColumnName = "columnName2",
                    RowKey = "rowKey2"
                });
            var element3 = new Tuple<string, ColumnInfo>("taskId3", new ColumnInfo
                {
                    ColumnName = "columnName3",
                    RowKey = "rowKey3"
                });
            CollectionAssert.AreEquivalent(new[] {element1}, tasksCache.PassThroughtCache(new[] {element1}));
            CollectionAssert.AreEquivalent(new[] {element1, element2}, tasksCache.PassThroughtCache(new[] {element2}));
            tasksCache.Remove(element2.Item1);
            CollectionAssert.AreEquivalent(new[] {element1}, tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
            CollectionAssert.AreEquivalent(new[] {element1, element3}, tasksCache.PassThroughtCache(new[] {element3}));
            tasksCache.Remove(element1.Item1);
            CollectionAssert.AreEquivalent(new[] {element3}, tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
            tasksCache.Remove(element3.Item1);
            CollectionAssert.IsEmpty(tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
        }

        [Test]
        public void TestRemovingNotExistsElement()
        {
            Assert.That(!tasksCache.Remove(Guid.NewGuid().ToString()));
            CollectionAssert.IsEmpty(tasksCache.PassThroughtCache(new Tuple<string, ColumnInfo>[0]));
        }

        private TasksCache tasksCache;
    }
}