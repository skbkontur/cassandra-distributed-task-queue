using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories;

namespace FunctionalTests.EventsEnumeratorTests
{
    public class EventsEnumerableTestBase : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            handleTasksMetaStorage = Container.Get<IHandleTasksMetaStorage>();
        }

        protected TaskMetaInformation[] GenerateMetas(int count)
        {
            var startTicks = DateTime.UtcNow.Ticks;
            var metas = new List<TaskMetaInformation>();
            for(int i = 1; i <= count; i++)
            {
                metas.Add(new TaskMetaInformation
                    {
                        Id = Guid.NewGuid().ToString(),
                        State = TaskState.New,
                        MinimalStartTicks = startTicks + i
                    });
            }
            return metas.ToArray();
        }

        protected IEnumerable<string> GetEnumerable(long toTicks, bool reverseOrder, params TaskState[] states)
        {
            return reverseOrder ? handleTasksMetaStorage.GetReverseAllTasksInStatesOrder(toTicks, states)
                       : handleTasksMetaStorage.GetAllTasksInStates(toTicks, states);
        }

        protected IHandleTasksMetaStorage handleTasksMetaStorage;
    }
}