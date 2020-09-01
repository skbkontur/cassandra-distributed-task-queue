using System;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedTaskQueue.Cassandra.Entities;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tracing
{
    public class RemoteTaskInitialTraceContext : IDisposable
    {
        public RemoteTaskInitialTraceContext([NotNull] TaskMetaInformation taskMeta)
        {
            /*traceContext = Trace.CreateChildContext(taskMeta.Name, taskMeta.Id);
            taskMeta.TraceId = traceContext.TraceId;
            taskMeta.TraceIsActive = traceContext.IsActive;
            traceContext.RecordTimepoint(Timepoint.Start, new DateTime(taskMeta.Ticks, DateTimeKind.Utc));*/
        }

        public void Dispose()
        {
            //traceContext.Dispose();
        }

        //private readonly ITraceContext traceContext;
    }
}