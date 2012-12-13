using RemoteQueue.Cassandra.Primitives;

namespace ExchangeService.UserClasses
{
    public interface ITestCassandraCounterBlobRepository : IBlobStorage<int>
    {
    }
}