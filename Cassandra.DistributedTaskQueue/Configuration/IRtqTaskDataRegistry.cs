using System;

#nullable enable

namespace SkbKontur.Cassandra.DistributedTaskQueue.Configuration
{
    public interface IRtqTaskDataRegistry
    {
        string[] GetAllTaskNames();
        
        string GetTaskName(Type type);
        
        Type GetTaskType(string taskName);

        bool TryGetTaskType(string taskName, out Type? taskType);
        
        string[] GetAllTaskTopics();

        (string TaskName, string TopicName)[] GetAllTaskNamesWithTopics();
        
        string GetTaskTopic(string taskName);
    }
}