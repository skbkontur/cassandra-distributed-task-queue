using System;

using RemoteQueue.Configuration;
using RemoteQueue.Handling;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatas
{
    [TaskName("ByteArrayAndNestedTaskData")]
    public class ByteArrayAndNestedTaskData : ITaskData
    {
        public string Value { get; set; }
        public string[] Values { get; set; }
        public string[][] ValuesOfValue { get; set; }
        public DateTime Date { get; set; }
        public DateTime? NullableDate { get; set; }
        public DateTime? NullableDate2 { get; set; }
        public NestedClass Nested { get; set; }
        public byte[][] BinaryMultiple { get; set; }
        public string ValueNull { get; set; }
        public NestedClass NestedNull { get; set; }
    }
}