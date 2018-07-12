using System;

using JetBrains.Annotations;

using SKBKontur.Catalogue.ServiceLib.Tracing;

namespace RemoteQueue.Profiling
{
    public class RtqGroboTraceProfilerSink : GroboTraceProfilerSink
    {
        public RtqGroboTraceProfilerSink([NotNull] string taskId)
            : base(logMessagePrefix : new Lazy<string>(() => string.Format("TaskId: {0}, ", taskId)))
        {
        }
    }
}