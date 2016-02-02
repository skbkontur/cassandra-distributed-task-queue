using System;
using System.Collections.Generic;
using System.Linq;

using GroboContainer.Core;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Configuration;

namespace FunctionalTests.ExchangeTests
{
    public static class TaskTestsExtensions
    {
        public static void CheckTaskMinimalStartTicksIndexStates(this IContainer container, Dictionary<string, TaskIndexShardKey> expectedShardKeys)
        {
            var globalTime = container.Get<IGlobalTime>();
            var index = container.Get<ITaskMinimalStartTicksIndex>();
            var allShardKeysForTasks = new Dictionary<string, List<TaskIndexShardKey>>();
            foreach(var taskTopic in container.Get<ITaskDataRegistry>().GetAllTaskTopics())
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
    }
}