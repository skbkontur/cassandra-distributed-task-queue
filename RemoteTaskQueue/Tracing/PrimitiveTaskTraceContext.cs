using System;

using JetBrains.Annotations;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tracing
{
    public abstract class PrimitiveTaskTraceContext : IDisposable
    {
        protected PrimitiveTaskTraceContext([NotNull] string primitiveName)
        {
            /*traceContext = Trace.CreateChildContext(primitiveName);
            traceContext.RecordTimepoint(Timepoint.Start);*/
        }

        public void Dispose()
        {
            /*traceContext.RecordTimepoint(Timepoint.Finish);
            traceContext.Dispose();*/
        }

        //private readonly ITraceContext traceContext;
    }
}