namespace ExchangeService.UserClasses
{
    public interface ITestCounterRepository
    {
        int GetCounter(string taskId);
        int IncrementCounter(string taskId);
        int DecrementCounter(string taskId);
        void SetValueForCounter(string taskId, int value);
    }
}