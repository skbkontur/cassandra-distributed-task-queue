using GroBuf;

using RemoteQueue.Cassandra.Entities;

using SKBKontur.Catalogue.CassandraPrimitives.RemoteLock;

namespace RemoteQueue.Handling
{
    public interface ITaskHandler
    {
        HandleResult HandleTask(IRemoteTaskQueue remoteTaskQueue, ISerializer serializer, IRemoteLockCreator remoteLockCreator, Task task);
    }
}