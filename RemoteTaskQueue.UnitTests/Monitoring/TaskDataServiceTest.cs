using System;

using NUnit.Framework;

using RemoteQueue.Configuration;

using RemoteTaskQueue.Monitoring.Storage.Writing;
using RemoteTaskQueue.Monitoring.Storage.Writing.Contracts;

using Rhino.Mocks;

using SKBKontur.Catalogue.TestCore;

namespace RemoteTaskQueue.UnitTests.Monitoring
{
    public class TaskDataServiceTest : CoreTestBase
    {
        public override void SetUp()
        {
            base.SetUp();
            taskDataRegistry = GetMock<ITaskDataRegistry>();
            taskDataService = new TaskDataService(taskDataRegistry);
        }

        [Test]
        public void TestUnknownType()
        {
            taskDataRegistry.Expect(m => m.TryGetTaskType(ARG.EqualsTo("zzz"), out ARG.Out<Type>(null).Dummy)).Return(false);
            var meta = new MetaIndexedInfo() {Name = "zzz"};
            taskDataService.CreateTaskIndexedInfo(meta, "zzz", null).AssertEqualsTo(new TaskIndexedInfo<TaskDataService.UnknownData>(meta, "zzz", null));
        }

        [Test]
        public void TestConstruct()
        {
            taskDataRegistry.Expect(m => m.TryGetTaskType(ARG.EqualsTo("C1"), out ARG.Out(typeof(C1)).Dummy)).Return(true);
            var meta = new MetaIndexedInfo() {Name = "C1"};
            var data = new C1 {X = 1};
            taskDataService.CreateTaskIndexedInfo(meta, null, data).AssertEqualsTo(new TaskIndexedInfo<C1>(meta, null, data));

            var data2 = new C1 {X = 1};
            taskDataService.CreateTaskIndexedInfo(meta, "zzz", data2).AssertEqualsTo(new TaskIndexedInfo<C1>(meta, "zzz", data2));

            taskDataRegistry.Expect(m => m.TryGetTaskType(ARG.EqualsTo("C2"), out ARG.Out(typeof(C2)).Dummy)).Return(true);
            var meta3 = new MetaIndexedInfo() {Name = "C2"};
            var data3 = new C2 {Z = 2};
            taskDataService.CreateTaskIndexedInfo(meta3, "zzz", data3).AssertEqualsTo(new TaskIndexedInfo<C2>(meta3, "zzz", data3));
        }

        private ITaskDataRegistry taskDataRegistry;
        private TaskDataService taskDataService;

        // ReSharper disable NotAccessedField.Local
        private class C1
        {
            public int X;
        }

        private class C2
        {
            public int Z;
        }

        // ReSharper restore NotAccessedField.Local
    }
}