using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SkbKontur.Cassandra.DistributedLock;

namespace RemoteQueue.Handling
{
    public interface ITaskHandler
    {
        HandleResult HandleTask(IRemoteTaskQueue remoteTaskQueue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task);
    }
}