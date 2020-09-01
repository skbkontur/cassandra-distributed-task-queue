namespace SkbKontur.Cassandra.DistributedTaskQueue.LocalTasks.TaskQueue
{
    public class LocalTaskQueueingResult
    {
        private LocalTaskQueueingResult(bool queueIsFull, bool queueIsStopped, bool taskIsSentToThreadPool)
        {
            QueueIsFull = queueIsFull;
            QueueIsStopped = queueIsStopped;
            TaskIsSentToThreadPool = taskIsSentToThreadPool;
        }

        public bool QueueIsFull { get; private set; }
        public bool QueueIsStopped { get; private set; }
        public bool TaskIsSentToThreadPool { get; private set; }

        public static readonly LocalTaskQueueingResult TaskIsSkippedResult = new LocalTaskQueueingResult(queueIsFull : false, queueIsStopped : false, taskIsSentToThreadPool : false);
        public static readonly LocalTaskQueueingResult QueueIsFullResult = new LocalTaskQueueingResult(queueIsFull : true, queueIsStopped : false, taskIsSentToThreadPool : false);
        public static readonly LocalTaskQueueingResult QueueIsStoppedResult = new LocalTaskQueueingResult(queueIsFull : false, queueIsStopped : true, taskIsSentToThreadPool : false);
        public static readonly LocalTaskQueueingResult SuccessResult = new LocalTaskQueueingResult(queueIsFull : false, queueIsStopped : false, taskIsSentToThreadPool : true);
    }
}