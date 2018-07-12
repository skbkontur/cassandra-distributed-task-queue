using System;

using Kontur.Tracing.Core;

namespace RemoteQueue.Tracing
{
    public class InfrastructureTaskTraceContext : IDisposable
    {
        public InfrastructureTaskTraceContext(bool taskIsBeingTraced)
        {
            if (taskIsBeingTraced)
            {
                traceContext = Trace.CreateChildContext("Handle.Infrastructure");
                traceContext.RecordTimepoint(Timepoint.Start);
            }
        }

        public void Finish(bool taskIsSentToThreadPool)
        {
            if (traceContext != null)
            {
                var flush = !taskIsSentToThreadPool;
                if (flush)
                    traceContext.RecordTimepoint(Timepoint.Finish);
                traceContext.Dispose(flush);
            }
        }

        public void Dispose()
        {
            if (traceContext != null)
                traceContext.Dispose();
        }

        public static void Finish()
        {
            TraceContext.Current.RecordTimepoint(Timepoint.Finish);
            Trace.FinishCurrentContext();
        }

        private readonly ITraceContext traceContext;
    }
}