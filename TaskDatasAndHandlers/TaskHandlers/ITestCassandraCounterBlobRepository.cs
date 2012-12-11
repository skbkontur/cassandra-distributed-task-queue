using RemoteQueue.Cassandra.Primitives;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatasAndHandlers.TaskHandlers
{
    public interface ITestCassandraCounterBlobRepository : IBlobStorage<int>
    {
    }
}