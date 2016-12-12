using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;

using RemoteTaskQueue.FunctionalTests.Common;

using SKBKontur.Catalogue.NUnit.Extensions.EdiTestMachinery;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.ExchangeTests
{
    [EdiTestSuite("ExchangeTests"), WithTestRemoteTaskQueue, AndResetExchangeServiceState, AndResetTicksHolderState]
    public abstract class ExchangeTestBase
    {
        protected TaskIndexShardKey TaskIndexShardKey(string taskName, TaskState taskState)
        {
            return new TaskIndexShardKey(taskDataRegistry.GetTaskTopic(taskName), taskState);
        }

        protected void CheckTaskMinimalStartTicksIndexStates(Dictionary<string, TaskIndexShardKey> expectedShardKeys)
        {
            var allShardKeysForTasks = new Dictionary<string, List<TaskIndexShardKey>>();
            foreach(var taskTopic in taskDataRegistry.GetAllTaskTopics())
            {
                foreach(var taskState in Enum.GetValues(typeof(TaskState)).Cast<TaskState>())
                {
                    var indexRecords = index.GetRecords(new TaskIndexShardKey(taskTopic, taskState), globalTime.GetNowTicks(), 2000).ToArray();
                    foreach(var indexRecord in indexRecords)
                    {
                        List<TaskIndexShardKey> shardKeys;
                        if(!allShardKeysForTasks.TryGetValue(indexRecord.TaskId, out shardKeys))
                        {
                            shardKeys = new List<TaskIndexShardKey>();
                            allShardKeysForTasks.Add(indexRecord.TaskId, shardKeys);
                        }
                        shardKeys.Add(indexRecord.TaskIndexShardKey);
                    }
                }
            }
            foreach(var allShardKeysForTask in allShardKeysForTasks)
            {
                var taskId = allShardKeysForTask.Key;
                var shardKeys = allShardKeysForTask.Value;
                if(shardKeys.Count > 1)
                    Assert.Fail("Task {0} found in several states in index: [{1}]", taskId, string.Join(",", shardKeys));
                if(expectedShardKeys.ContainsKey(taskId))
                    Assert.AreEqual(expectedShardKeys[taskId], shardKeys.Single(), "Ожидаемое и фактическое состояния таски {0} не совпали", taskId);
                else
                    Assert.Fail("Found task {0} in index, but not found in expected tasks", taskId);
            }
            foreach(var kvp in expectedShardKeys)
            {
                var taskId = kvp.Key;
                var expectedShardKey = kvp.Value;
                if(allShardKeysForTasks.ContainsKey(taskId))
                    Assert.AreEqual(expectedShardKey, allShardKeysForTasks[taskId].Single(), "Ожидаемое и фактическое состояния таски {0} не совпали", taskId);
                else
                    Assert.Fail("Expected task {0} not found in index", taskId);
            }
        }

        [Injected]
        private readonly IGlobalTime globalTime;

        [Injected]
        private readonly ITaskMinimalStartTicksIndex index;

        [Injected]
        private readonly ITaskDataRegistry taskDataRegistry;

        [Injected]
        [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
        protected readonly RemoteQueue.Handling.RemoteTaskQueue remoteTaskQueue;
    }
}