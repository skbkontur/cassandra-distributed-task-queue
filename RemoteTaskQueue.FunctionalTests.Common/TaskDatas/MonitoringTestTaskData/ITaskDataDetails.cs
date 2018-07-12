using System;

using GroBuf;

using JetBrains.Annotations;

using SKBKontur.Catalogue.GrobufExtensions;
using SKBKontur.Catalogue.Objects;

namespace RemoteTaskQueue.FunctionalTests.Common.TaskDatas.MonitoringTestTaskData
{
    public class ContentTypeNameAttribute : Attribute, ITypeNameAttribute
    {
        public ContentTypeNameAttribute([NotNull] string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new InvalidProgramStateException("TypeName is empty");
            Type = typeName;
        }

        [NotNull]
        public string Type { get; }
    }

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