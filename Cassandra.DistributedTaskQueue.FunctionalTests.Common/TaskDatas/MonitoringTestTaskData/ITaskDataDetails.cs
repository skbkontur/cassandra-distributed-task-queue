using GroBuf;

namespace SkbKontur.Cassandra.DistributedTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    public interface ITaskDataDetails
    {
    }

    [GroBufCustomSerialization(typeof(DerivedTypesSerialization<TaskDataDetailsBase, ContentTypeNameAttribute>))]
    public abstract class TaskDataDetailsBase : ITaskDataDetails
    {
    }

    [ContentTypeName(nameof(PlainTaskDataDetails))]
    public class PlainTaskDataDetails : TaskDataDetailsBase
    {
        public string Info { get; set; }
    }

    [ContentTypeName(nameof(StructuredTaskDataDetails))]
    public class StructuredTaskDataDetails : TaskDataDetailsBase
    {
        public DetailsInfo Info { get; set; }

        public class DetailsInfo
        {
            public string SomeInfo { get; set; }
        }
    }
}