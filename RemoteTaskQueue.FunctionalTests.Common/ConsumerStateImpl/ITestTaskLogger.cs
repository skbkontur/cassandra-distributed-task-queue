namespace RemoteTaskQueue.FunctionalTests.Common.ConsumerStateImpl
{
    public interface ITestTaskLogger
    {
        void Log(string loggingTaskIdKey, string taskId);
        string[] GetAll(string loggingTaskIdKey);
    }
}