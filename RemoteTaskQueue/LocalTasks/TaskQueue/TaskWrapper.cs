using System;

using Kontur.Tracing.Core;

using log4net;

using RemoteQueue.Handling;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskWrapper
    {
        public TaskWrapper(string taskId, HandlerTask handlerTask, LocalTaskQueue localTaskQueue)
        {
            this.taskId = taskId;
            this.handlerTask = handlerTask;
            this.localTaskQueue = localTaskQueue;
            finished = false;
        }

        public bool Finished { get { return finished; } }

        public void Run()
        {
            LocalTaskProcessingResult result;
            try
            {
                result = handlerTask.RunTask();
            }
            catch(Exception e)
            {
                result = LocalTaskProcessingResult.Undefined;
                logger.Error("Ошибка во время обработки асинхронной задачи.", e);
            }

            try
            {
                finished = true;
                localTaskQueue.TaskFinished(taskId);

                TraceContext.Current.RecordTimepoint(Timepoint.Finish);
                Trace.FinishCurrentContext(); // Finish infrastructureTraceContext

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
                Trace.FinishCurrentContext(); // Finish RemoteTaskTraceContext
            }
            catch(Exception e)
            {
                logger.Warn("Ошибка во время окончания задачи.", e);
            }
        }

        private readonly string taskId;
        private readonly HandlerTask handlerTask;
        private readonly LocalTaskQueue localTaskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(LocalTaskQueue));
    }
}