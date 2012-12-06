
using GroBuf;

using RemoteQueue.Cassandra.Entities;
using RemoteQueue.Cassandra.RemoteLock;
using RemoteQueue.Handling.HandlerResults;

namespace RemoteQueue.Handling
{
    public interface ITaskHandler
    {
        HandleResult HandleTask(IRemoteTaskQueue remoteTaskQueue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task);
    }
}