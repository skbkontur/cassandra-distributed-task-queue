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
            if (taskMeta != null)
            {
                traceContext = Trace.ContinueContext(taskMeta.TraceId, taskMeta.Id, taskMeta.TraceIsActive, isRoot : true);
                traceContext.RecordTimepoint(Timepoint.Start, new DateTime(taskMeta.Ticks, DateTimeKind.Utc));
            }
        }

        public bool TaskIsBeingTraced { get; private set; }

        public void Finish(bool taskIsSentToThreadPool, Func<long> getGlobalNowTicks)
        {
            if (traceContext != null)
            {
                var flush = !taskIsSentToThreadPool;
                if (flush)
                    traceContext.RecordTimepoint(Timepoint.Finish, new DateTime(getGlobalNowTicks(), DateTimeKind.Utc));
                traceContext.Dispose(flush);
            }
        }

        public void Dispose()
        {
            if (traceContext != null)
                traceContext.Dispose();
        }

        public static void Finish(LocalTaskProcessingResult result, long finishTaskProcessingTicks)
        {
            switch (result)
            {
            case LocalTaskProcessingResult.Success:
                TraceContext.Current.RecordAnnotation(Annotation.ResponseCode, "200");
                TraceContext.Current.RecordAnnotation(Annotation.Revision, finishTaskProcessingTicks.ToString());
                break;
            case LocalTaskProcessingResult.Error:
                TraceContext.Current.RecordAnnotation(Annotation.ResponseCode, "500");
                TraceContext.Current.RecordAnnotation(Annotation.Revision, finishTaskProcessingTicks.ToString());
                break;
            case LocalTaskProcessingResult.Rerun:
                TraceContext.Current.RecordAnnotation(Annotation.ResponseCode, "400");
                TraceContext.Current.RecordAnnotation(Annotation.Revision, finishTaskProcessingTicks.ToString());
                break;
            case LocalTaskProcessingResult.Undefined:
                break;
            default:
                throw new InvalidProgramStateException(string.Format("Invalid LocalTaskProcessingResult: {0}", result));
            }
            TraceContext.Current.RecordTimepoint(Timepoint.Finish, new DateTime(finishTaskProcessingTicks, DateTimeKind.Utc));
            Trace.FinishCurrentContext();
        }

        private readonly ITraceContext traceContext;
    }
}