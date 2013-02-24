namespace RemoteLock
{
    public enum LockAttemptStatus
    {
        Success,
        AnotherThreadIsOwner,
        ConcurrentAttempt
    }
}