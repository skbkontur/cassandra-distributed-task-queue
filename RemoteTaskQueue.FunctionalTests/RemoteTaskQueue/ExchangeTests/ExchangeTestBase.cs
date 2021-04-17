using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using GroboContainer.NUnitExtensions;

using NUnit.Framework;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using SkbKontur.Cassandra.DistributedTaskQueue.Configuration;
using SkbKontur.Cassandra.GlobalTimestamp;

namespace RemoteTaskQueue.FunctionalTests.RemoteTaskQueue.ExchangeTests
{
    [GroboTestSuite("ExchangeTests"), WithTestRemoteTaskQueue, AndResetExchangeServiceState]
    public abstract class ExchangeTestBase
    {
        protected TaskIndexShardKey TaskIndexShardKey(string taskName, TaskState taskState)
        {
            return new TaskIndexShardKey(taskDataRegistry.GetTaskTopic(taskName), taskState);
        }

        protected void CheckTaskMinimalStartTicksIndexStates(Dictionary<string, TaskIndexShardKey> expectedShardKeys)
        {
            var allShardKeysForTasks = new Dictionary<string, List<TaskIndexShardKey>>();
            foreach (var taskTopic in taskDataRegistry.GetAllTaskTopics())
            {
                foreach (var taskState in Enum.GetValues(typeof(TaskState)).Cast<TaskState>())
                {
                    var indexRecords = index.GetRecords(new TaskIndexShardKey(taskTopic, taskState), globalTime.UpdateNowTimestamp().Ticks, batchSize : 2000).ToArray();
                    foreach (var indexRecord in indexRecords)
                    {
                        List<TaskIndexShardKey> shardKeys;
                        if (!allShardKeysForTasks.TryGetValue(indexRecord.TaskId, out shardKeys))
                        {
                            shardKeys = new List<TaskIndexShardKey>();
                            allShardKeysForTasks.Add(indexRecord.TaskId, shardKeys);
                        }
                        shardKeys.Add(indexRecord.TaskIndexShardKey);
                    }
                }
            }
            foreach (var allShardKeysForTask in allShardKeysForTasks)
            {
                var taskId = allShardKeysForTask.Key;
                var shardKeys = allShardKeysForTask.Value;
                if (shardKeys.Count > 1)
                    Assert.Fail("Task {0} found in several states in index: [{1}]", taskId, string.Join(",", shardKeys));
                if (expectedShardKeys.ContainsKey(taskId))
                    Assert.AreEqual(expectedShardKeys[taskId], shardKeys.Single(), "Ожидаемое и фактическое состояния таски {0} не совпали", taskId);
                else
                    Assert.Fail("Found task {0} in index, but not found in expected tasks", taskId);
            }
            foreach (var kvp in expectedShardKeys)
            {
                var taskId = kvp.Key;
                var expectedShardKey = kvp.Value;
                if (allShardKeysForTasks.ContainsKey(taskId))
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
        private readonly IRtqTaskDataRegistry taskDataRegistry;

        [Injected]
        [SuppressMessage("ReSharper", "UnassignedReadonlyField")]
        protected readonly SkbKontur.Cassandra.DistributedTaskQueue.Handling.RemoteTaskQueue remoteTaskQueue;
    }
}