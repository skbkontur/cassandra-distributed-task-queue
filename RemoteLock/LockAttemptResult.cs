namespace RemoteLock
{
    public class LockAttemptResult
    {
        private LockAttemptResult(LockAttemptStatus status, string ownerId)
        {
            Status = status;
            OwnerId = ownerId;
        }

        public static LockAttemptResult Success()
        {
            return new LockAttemptResult(LockAttemptStatus.Success, null);
        }

        public static LockAttemptResult AnotherOwner(string ownerId)
        {
            return new LockAttemptResult(LockAttemptStatus.AnotherThreadIsOwner, ownerId);
        }

        public static LockAttemptResult ConcurrentAttempt()
        {
            return new LockAttemptResult(LockAttemptStatus.ConcurrentAttempt, null);
        }

        public LockAttemptStatus Status { get; private set; }
        public string OwnerId { get; private set; }
    }
}