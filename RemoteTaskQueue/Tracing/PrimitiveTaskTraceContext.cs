using System;

using Kontur.Tracing.Core;

namespace RemoteQueue.Tracing
{
    public abstract class PrimitiveTaskTraceContext : IDisposable
    {
        protected PrimitiveTaskTraceContext(string primitiveName)
        {
            traceContext = Trace.CreateChildContext(primitiveName);
            traceContext.RecordTimepoint(Timepoint.Start);
        }

        public void Dispose()
        {
            traceContext.RecordTimepoint(Timepoint.Finish);
            traceContext.Dispose();
        }

        private readonly ITraceContext traceContext;
    }
}