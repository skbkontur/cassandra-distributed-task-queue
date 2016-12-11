namespace ExchangeService.UserClasses
{
    public interface ITestTaskLogger
    {
        void Log(string loggingTaskIdKey, string taskId);
        string[] GetAll(string loggingTaskIdKey);
    }
}