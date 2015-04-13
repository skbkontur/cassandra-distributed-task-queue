using System;
using System.Collections.Generic;

using RemoteQueue.Cassandra.Entities;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.MvcControllers.Models
{
    public class TaskSearchConditionsModel
    {
        public string AdminToolAction { get; set; }
        public string[] AvailableTaskDataNames { get; set; }
        public KeyValuePair<string, string>[] AvailableTaskStates { get; set; }
        public string SearchString { get; set; }
        public string[] TaskNames { get; set; }
        public string[] TaskStates { get; set; }
        public DateTime? RangeStart { get; set; }
        public DateTime? RangeEnd { get; set; }
    }

    public class TasksResultModel
    {
        public TaskSearchConditionsModel SearchConditions { get; set; }        
        public TaskSearchResultsModel Results { get; set; }
    }

    public class TaskSearchResultsModel
    {
        public TaskModel[] Tasks { get; set; }
        public string IteratorContext { get; set; }
        public int TotalResultCount { get; set; }
        public bool AllowControlTaskExecution { get; set; }
        public bool AllowViewTaskData { get; set; }
    }

    public class TaskModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TaskState State { get; set; }
        public DateTime? EnqueueTime { get; set; }
        public DateTime? StartExecutionTime { get; set; }
        public DateTime? FinishExecutionTime { get; set; }
        public DateTime? MinimalStartTime { get; set; }
        public int AttemptCount { get; set; }
        public string ParentTaskId { get; set; }
    }
}