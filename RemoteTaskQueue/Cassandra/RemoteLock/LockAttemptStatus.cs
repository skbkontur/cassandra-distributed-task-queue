namespace RemoteQueue.Cassandra.RemoteLock
{
    public enum LockAttemptStatus
    {
        Success,
        AnotherThreadIsOwner,
        ConcurrentAttempt
    }
}