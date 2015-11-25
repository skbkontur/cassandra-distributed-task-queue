namespace RemoteQueue.Configuration
{
    public interface IExchangeSchedulableRunner
    {
        void Start();
        void Stop();
    }
}