using System;

namespace RemoteQueue.Cassandra.Primitives
{
    public static class RemoteTaskQueueLockConstants
    {
        [Obsolete("burmistrov: Remove it after remote lock migration is completed")]
        public const string LockColumnFamily = "lock";

        public const string NewLockColumnFamily = "RemoteTaskQueueLock";
    }
}