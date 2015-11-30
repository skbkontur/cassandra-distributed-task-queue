using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.BlobStorages;

namespace ExchangeService.UserClasses
{
    public interface ITestCassandraCounterBlobRepository : IBlobStorage<int>
    {
    }
}