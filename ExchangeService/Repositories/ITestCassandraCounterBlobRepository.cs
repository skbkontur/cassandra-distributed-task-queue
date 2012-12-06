using RemoteQueue.Cassandra.Primitives;

namespace ExchangeService.Repositories
{
    public interface ITestCassandraCounterBlobRepository : IBlobStorage<int>
    {
    }
}