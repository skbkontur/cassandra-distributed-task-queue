namespace RemoteLock
{
    public class RemoteLockCreator : IRemoteLockCreator
    {
        public RemoteLockCreator(ILockRepository lockRepository)
        {
            this.lockRepository = lockRepository;
        }

        public IRemoteLock Lock(string lockId)
        {
            return new RemoteLock(lockRepository, lockId);
        }

        public bool TryGetLock(string lockId, out IRemoteLock remoteLock)
        {
            string concurrentThreadId;
            var weakRemoteLock = new WeakRemoteLock(lockRepository, lockId, out concurrentThreadId);
            remoteLock = weakRemoteLock;
            return string.IsNullOrEmpty(concurrentThreadId);
        }

        private readonly ILockRepository lockRepository;
    }
}