namespace RemoteQueue.LocalTasks.Scheduling
{
    public interface ISmartTimer
    {
        void StopAndWait(int timeout);
    }
}