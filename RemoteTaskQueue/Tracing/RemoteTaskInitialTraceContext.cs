using System;

using JetBrains.Annotations;

using Kontur.Tracing.Core;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Tracing
{
    public class RemoteTaskInitialTraceContext : IDisposable
    {
        public RemoteTaskInitialTraceContext([NotNull] TaskMetaInformation taskMeta)
        {
            traceContext = Trace.CreateChildContext(taskMeta.Name, taskMeta.Id);
            taskMeta.TraceId = traceContext.TraceId;
            taskMeta.TraceIsActive = traceContext.IsActive;
            traceContext.RecordTimepoint(Timepoint.Start, new DateTime(taskMeta.Ticks, DateTimeKind.Utc));
        }

        public void Dispose()
        {
            traceContext.Dispose();
        }

        private readonly ITraceContext traceContext;
    }
}