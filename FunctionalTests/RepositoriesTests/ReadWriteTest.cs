using System;
using System.Linq;
using System.Threading;

using ExchangeService.UserClasses;

using NUnit.Framework;

using RemoteQueue.Settings;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

using log4net;

namespace FunctionalTests.RepositoriesTests
{
    public class ReadWriteTest : FunctionalTestBaseWithoutServices
    {
        public override void SetUp()
        {
            base.SetUp();
            settings = Container.Get<ICassandraSettings>();
            cassandraCluster = Container.Get<ICassandraCluster>();
            connection = cassandraCluster.RetrieveColumnFamilyConnection(settings.QueueKeyspace, TestCassandraCounterBlobRepository.columnFamilyName);
            logger = LogManager.GetLogger(typeof(ReadWriteTest));
        }

        [Test]
        public void TestReadWrite()
        {
            threads = new Thread[threadsCount];
            for(int i = 0; i < threadsCount; i++)
                threads[i] = new Thread(ThreadAction);
            for(int i = 0; i < threadsCount; i++)
                threads[i].Start();
            const int secondsCount = 10;
            for(int i = 0; i < secondsCount; i++)
            {
                Thread.Sleep(1000);
                for(int j = 0; j < threadsCount; j++)
                    Assert.That(threads[j].IsAlive);
            }
        }

        public override void TearDown()
        {
            stop = true;
            for(int i = 0; i < threadsCount; i++)
                threads[i].Join();
            base.TearDown();
        }

        public void ThreadAction()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            while(true)
            {
                if(stop) return;
                try
                {
                    var guid = Guid.NewGuid().ToString();
                    var row = "row" + random.Next(2);
                    Add(row, guid);
                    if(!CheckIn(row, guid))
                        throw new Exception("bug");
                    Delete(row, guid);
                }
                catch(Exception e)
                {
                    logger.Error(e);
                    throw;
                }
            }
        }

        private void Add(string row, string id)
        {
            connection.AddColumn(row, new Column
                {
                    Name = id,
                    Timestamp = 0,
                    Value = new byte[] {0}
                });
        }

        private void Delete(string row, string id)
        {
            connection.DeleteBatch(row, new[] {id}, 1);
        }

        private bool CheckIn(string row, string id)
        {
            var ids = connection.GetRow(row).Select(t => t.Name).ToArray();
            var res = ids.Any(t => t == id);
            if(!res)
                logger.Info("Was [" + row + "]:\n" + string.Join(",\n", ids) + "\nNeeded:\n" + id + "\n");
            return res;
        }

        private volatile bool stop;
        private ICassandraSettings settings;
        private ICassandraCluster cassandraCluster;
        private IColumnFamilyConnection connection;
        private ILog logger;
        private Thread[] threads;
        private const int threadsCount = 30;
    }
}