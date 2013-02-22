
using GroBuf;

using RemoteLock;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Handling
{
    public interface ITaskHandler
    {
        HandleResult HandleTask(IRemoteTaskQueue remoteTaskQueue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task);
    }
}