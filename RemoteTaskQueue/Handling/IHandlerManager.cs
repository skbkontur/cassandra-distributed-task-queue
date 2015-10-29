using SKBKontur.Catalogue.ServiceLib.Scheduling;

namespace RemoteQueue.Handling
{
    public interface IHandlerManager : IPeriodicTask
    {
        void Start();
        void Stop();
    }
}