using System;

using Kontur.Tracing.Core;

using RemoteQueue.Cassandra.Entities;

namespace RemoteQueue.Tracing
{
    public class RemoteTaskTraceContext : IDisposable
    {
        public RemoteTaskTraceContext(TaskMetaInformation taskMeta)
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