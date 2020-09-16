using System;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace SkbKontur.Cassandra.DistributedTaskQueue.TestApi.Controllers
{
    public enum TestEnum
    {
        Value1,
        Value2,
    }

    public class TaskData : IRtqTaskData
    {
        public TestEnum TestEnum { get; set; }
        public string DocumentCirculationId { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
    }
}