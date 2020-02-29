using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using SkbKontur.Cassandra.DistributedLock;

using SKBKontur.Catalogue.ServiceLib.Logging;

using Vostok.Logging.Abstractions;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Handling
{
    [Obsolete("// todo (andrew, 01.03.2020): remove after avk/rtqLock release")]
    public class MigratingRemoteLocker : IRemoteLockCreator
    {
        public MigratingRemoteLocker(IRemoteLockCreator oldRemoteLocker, IRemoteLockCreator newRemoteLocker)
        {
            this.oldRemoteLocker = oldRemoteLocker;
            this.newRemoteLocker = newRemoteLocker;
        }

        [NotNull]
        public IRemoteLock Lock([NotNull] string lockId)
        {
            var oldLock = oldRemoteLocker.Lock(lockId);
            IRemoteLock newLock = null;
            try
            {
                newLock = newRemoteLocker.Lock(lockId);
                return new CompositeRemoteLock(oldLock, newLock);
            }
            catch (Exception e)
            {
                Log.For(this).Error(e, $"Exception during Lock of new lock after successfully took old one. LockId = {lockId}");
                throw;
            }
            finally
            {
                if (newLock == null)
                    oldLock.Dispose();
            }
        }

        public bool TryGetLock([NotNull] string lockId, out IRemoteLock remoteLock)
        {
            if (!oldRemoteLocker.TryGetLock(lockId, out var oldLock))
            {
                remoteLock = null;
                return false;
            }
            IRemoteLock newLock = null;
            try
            {
                if (newRemoteLocker.TryGetLock(lockId, out newLock))
                {
                    remoteLock = new CompositeRemoteLock(oldLock, newLock);
                    return true;
                }
            }
            catch (Exception e)
            {
                Log.For(this).Error(e, $"Exception during TryGetLock of new lock after successfully took old one. LockId = {lockId}");
                throw;
            }
            finally
            {
                if (newLock == null)
                    oldLock.Dispose();
            }
            remoteLock = null;
            return false;
        }

        private readonly IRemoteLockCreator oldRemoteLocker;
        private readonly IRemoteLockCreator newRemoteLocker;

        private class CompositeRemoteLock : IRemoteLock
        {
            public CompositeRemoteLock([NotNull] IRemoteLock oldRemoteLock, [NotNull] IRemoteLock newRemoteLock)
            {
                this.oldRemoteLock = oldRemoteLock;
                this.newRemoteLock = newRemoteLock;
            }

            public void Dispose()
            {
                List<Exception> exceptions = null;

                if (!TryDispose(newRemoteLock, out var newRemoteLockException))
                    exceptions = AddToList(null, newRemoteLockException);

                if (!TryDispose(oldRemoteLock, out var oldRemoteLockException))
                    exceptions = AddToList(exceptions, oldRemoteLockException);

                if (exceptions != null)
                {
                    var message = $"Error while Disposing CompositeRemoteLock. Old lock: [LockId = {oldRemoteLock.LockId}, ThreadId = {oldRemoteLock.ThreadId}], new lock: [LockId = {newRemoteLock.LockId}, ThreadId = {newRemoteLock.ThreadId}]";
                    throw new AggregateException(message, exceptions);
                }
            }

            [CanBeNull]
            private static List<Exception> AddToList([CanBeNull] List<Exception> list, [NotNull] Exception exception)
            {
                if (list == null)
                    list = new List<Exception>();
                list.Add(exception);
                return list;
            }

            private static bool TryDispose([NotNull] IRemoteLock remoteLock, out Exception exception)
            {
                exception = null;
                try
                {
                    remoteLock.Dispose();
                }
                catch (Exception e)
                {
                    exception = e;
                    return false;
                }
                return true;
            }

            public string LockId => oldRemoteLock.LockId;
            public string ThreadId => oldRemoteLock.ThreadId;

            private readonly IRemoteLock oldRemoteLock;
            private readonly IRemoteLock newRemoteLock;
        }
    }
}