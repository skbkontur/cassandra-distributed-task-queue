using System;

using Kontur.Tracing.Core;

using log4net;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.LocalTasks.TaskQueue
{
    public class TaskWrapper
    {
        public TaskWrapper(ITask task, TaskQueue taskQueue)
        {
            this.task = task;
            this.taskQueue = taskQueue;
            finished = false;
        }

        public void Run()
        {
            LocalTaskProcessingResult result;
            try
            {
                result = task.RunTask();
            }
            catch(Exception e)
            {
                result = LocalTaskProcessingResult.Undefined;
                logger.Error("Ошибка во время обработки асинхронной задачи.", e);
            }

            try
            {
                finished = true;
                taskQueue.TaskFinished(task);

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

        public bool Finished { get { return finished; } }
        private readonly ITask task;
        private readonly TaskQueue taskQueue;
        private volatile bool finished;
        private readonly ILog logger = LogManager.GetLogger(typeof(TaskQueue));
    }
}