namespace RemoteQueue.Handling
{
    public interface IShardingManager
    {
        bool IsSituableTask(string taskId);
    }
}