using System;
using System.Collections.Generic;
using System.Linq;

using GroboContainer.Core;

using NUnit.Framework;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.Repositories.Indexes;
using RemoteQueue.Cassandra.Repositories.Indexes.StartTicksIndexes;
using RemoteQueue.Handling;

namespace FunctionalTests.ExchangeTests
{
    public static class TaskTestsExtensions
    {
        public static void CheckTaskMinimalStartTicksIndexStates(this IContainer container, Dictionary<string, TaskTopicAndState> expectedStates)
        {
            var index = container.Get<ITaskMinimalStartTicksIndex>();
            var allStatesForTasks = new Dictionary<string, List<TaskTopicAndState>>();
            foreach(var taskTopic in container.Get<TaskDataTypeToNameMapper>().GetAllTaskNames())
            {
                foreach(var taskState in Enum.GetValues(typeof(TaskState)).Cast<TaskState>())
                {
                    var indexRecords = index.GetRecords(new TaskTopicAndState(taskTopic, taskState), DateTime.UtcNow.Ticks, 2000).ToArray();
                    foreach(var indexRecord in indexRecords)
                    {
                        List<TaskTopicAndState> states;
                        if(allStatesForTasks.ContainsKey(indexRecord.TaskId))
                            states = allStatesForTasks[indexRecord.TaskId];
                        else
                        {
                            states = new List<TaskTopicAndState>();
                            allStatesForTasks[indexRecord.TaskId] = states;
                        }
                        states.Add(indexRecord.TaskTopicAndState);
                    }
                }
            }
            foreach(var allStatesForTask in allStatesForTasks)
            {
                var taskId = allStatesForTask.Key;
                var taskStates = allStatesForTask.Value;
                if(taskStates.Count > 1)
                    Assert.Fail("Task {0} found in several states in index: [{1}]", taskId, string.Join(",", taskStates));
                if(expectedStates.ContainsKey(taskId))
                    Assert.AreEqual(expectedStates[taskId], taskStates.Single(), "Ожидаемое и фактическое состояния таски {0} не совпали", taskId);
                else
                    Assert.Fail("Found task {0} in index, but not found in expected tasks", taskId);
            }
            foreach(var taskAndState in expectedStates)
            {
                var taskId = taskAndState.Key;
                var expectedState = taskAndState.Value;
                if(allStatesForTasks.ContainsKey(taskId))
                    Assert.AreEqual(expectedState, allStatesForTasks[taskId].Single(), "Ожидаемое и фактическое состояния таски {0} не совпали", taskId);
                else
                    Assert.Fail("Expected task {0} not found in index", taskId);
            }
        }
    }
}