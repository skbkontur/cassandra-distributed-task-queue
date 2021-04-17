using System;

using GroboContainer.Infection;

using GroBuf;
using GroBuf.DataMembersExtracters;

using SkbKontur.Cassandra.DistributedTaskQueue.Handling;

namespace RemoteTaskQueue.FunctionalTests.Common
{
    [IgnoredImplementation]
    public class TestRtqSettings : IRtqSettings
    {
        public string QueueKeyspace { get; } = QueueKeyspaceName;
        public TimeSpan TaskTtl { get; } = StandardTestTaskTtl;
        public bool EnableContinuationOptimization { get; } = true;

        public const string QueueKeyspaceName = "TestRtqKeyspace";

        public static readonly TimeSpan StandardTestTaskTtl = TimeSpan.FromHours(24);

        public static readonly ISerializer MergeOnReadInstance = new Serializer(new AllPropertiesExtractor(), new DefaultGroBufCustomSerializerCollection(), GroBufOptions.MergeOnRead);
    }
}