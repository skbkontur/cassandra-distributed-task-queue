namespace SKBKontur.Catalogue.RemoteTaskQueue.Common
{
    public interface IExchangeServiceClient
    {
        void Start();
        void Stop();
    }
}