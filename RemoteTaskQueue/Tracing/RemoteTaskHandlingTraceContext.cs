using System;

using JetBrains.Annotations;

using Kontur.Tracing.Core;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.LocalTasks.TaskQueue;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Tracing
{
    public class RemoteTaskHandlingTraceContext : IDisposable
    {
        public RemoteTaskHandlingTraceContext([CanBeNull] TaskMetaInformation taskMeta)
        {
            TaskIsBeingTraced = taskMeta != null;
            if(taskMeta != null)
            {
                traceContext = Trace.ContinueContext(taskMeta.TraceId, taskMeta.Id, taskMeta.TraceIsActive, isRoot : true);
                traceContext.RecordTimepoint(Timepoint.Start, new DateTime(taskMeta.Ticks, DateTimeKind.Utc));
            }
        }

        public bool TaskIsBeingTraced { get; private set; }

        public void Dispose()
        {
            if(traceContext != null)
                traceContext.Dispose(); // pop Task trace context
        }

        public static void Finish(LocalTaskProcessingResult result)
        {
            switch(result)
            {
            case LocalTaskProcessingResult.Success:
                TraceContext.Current.RecordAnnotation(Annotation.ResponseCode, "200");
                break;
            case LocalTaskProcessingResult.Error:
                TraceContext.Current.RecordAnnotation(Annotation.ResponseCode, "500");
                break;
            case LocalTaskProcessingResult.Rerun:
                TraceContext.Current.RecordAnnotation(Annotation.ResponseCode, "400");
                break;
            case LocalTaskProcessingResult.Undefined:
                break;
            default:
                throw new InvalidProgramStateException(string.Format("Invalid LocalTaskProcessingResult: {0}", result));
            }
            TraceContext.Current.RecordTimepoint(Timepoint.Finish);
            Trace.FinishCurrentContext(); // finish Task trace context
        }

        private readonly ITraceContext traceContext;
    }
}