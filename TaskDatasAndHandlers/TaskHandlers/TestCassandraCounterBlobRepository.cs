using GroBuf;

using RemoteQueue.Cassandra.Primitives;
using RemoteQueue.Cassandra.Repositories.GlobalTicksHolder;

namespace SKBKontur.Catalogue.RemoteTaskQueue.TaskDatasAndHandlers.TaskHandlers
{
    public class TestCassandraCounterBlobRepository : BlobStorage<int>, ITestCassandraCounterBlobRepository
    {
        public TestCassandraCounterBlobRepository(IColumnFamilyRepositoryParameters parameters, ISerializer serializer, IGlobalTime globalTime)
            : base(parameters, serializer, globalTime, columnFamilyName)
        {
        }

        public const string columnFamilyName = "columnFamilyName";
    }
}