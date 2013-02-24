namespace RemoteLock
{
    public interface ILockRepository
    {
        LockAttemptResult TryLock(string lockId, string threadId);
        void Unlock(string lockId, string threadId);
        void Relock(string lockId, string threadId);
        string[] GetLockThreads(string lockId);
        string[] GetShadeThreads(string lockId);
    }
}