namespace RemoteLock
{
    public interface IRemoteLockCreator
    {
        IRemoteLock Lock(string lockId);
        bool TryGetLock(string lockId, out IRemoteLock remoteLock);
    }
}