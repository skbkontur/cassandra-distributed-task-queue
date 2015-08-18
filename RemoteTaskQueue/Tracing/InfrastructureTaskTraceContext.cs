using System;

using Kontur.Tracing.Core;

namespace RemoteQueue.Tracing
{
    public class InfrastructureTaskTraceContext : IDisposable
    {
        public InfrastructureTaskTraceContext(bool taskIsBeingTraced)
        {
            if(taskIsBeingTraced)
            {
                traceContext = Trace.CreateChildContext("Handle.Infrastructure");
                traceContext.RecordTimepoint(Timepoint.Start);
            }
        }

        public void RecordFinish()
        {
            if(traceContext != null)
                traceContext.RecordTimepoint(Timepoint.Finish);
        }

        public void Dispose()
        {
            if(traceContext != null)
                traceContext.Dispose(); // pop / finish Handle.Infrastructure trace context
        }

        public static void Finish()
        {
            TraceContext.Current.RecordTimepoint(Timepoint.Finish);
            Trace.FinishCurrentContext(); // finish Handle.Infrastructure trace context
        }

        private readonly ITraceContext traceContext;
    }
}