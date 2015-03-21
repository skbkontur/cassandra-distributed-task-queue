using SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.DataTypes;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskCounter.Core.Implementation
{
    public interface ICounterController
    {
        void Restart(long? newStartTicks);
        TaskCount GetTotalCount();
        void ProcessNewEvents();
    }
}