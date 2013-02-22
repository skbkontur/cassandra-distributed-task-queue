using System;
using System.Linq;
using System.Threading;

using SKBKontur.Cassandra.CassandraClient.Abstractions;
using SKBKontur.Cassandra.CassandraClient.Clusters;
using SKBKontur.Cassandra.CassandraClient.Connections;

namespace RemoteLock
{
    public class LockRepository : ILockRepository
    {
        public LockRepository(ICassandraCluster cassandraCluster, string keyspace, string columnFamily)
        {
            this.cassandraCluster = cassandraCluster;
            this.keyspace = keyspace;
            this.columnFamily = columnFamily;
        }

        public LockAttemptResult TryLock(string lockId, string threadId)
        {
            var items = GetLockThreads(lockId);
            if(items.Length == 1)
                return items[0] == threadId ? LockAttemptResult.Success() : LockAttemptResult.AnotherOwner(items[0]);
            if(items.Length > 1)
            {
                if(items.Any(s => s == threadId))
                    throw new Exception("Lock unknown exception");
                return LockAttemptResult.AnotherOwner(items[0]);
            }

            Write(GetShadeRow(lockId), threadId);
            var shades = GetShadeThreads(lockId);
            if(shades.Length == 1)
            {
                items = GetLockThreads(lockId);
                if(items.Length == 0)
                {
                    Write(GetMainRow(lockId), threadId);
                    Delete(GetShadeRow(lockId), threadId);
                    return LockAttemptResult.Success();
                }
            }
            Delete(GetShadeRow(lockId), threadId);
            return LockAttemptResult.ConcurrentAttempt();
        }

        public void Unlock(string lockId, string threadId)
        {
            Delete(GetMainRow(lockId), threadId);
        }

        public void Relock(string lockId, string threadId)
        {
            Write(GetMainRow(lockId), threadId);
        }

        public string[] GetLockThreads(string lockId)
        {
            return Search(GetMainRow(lockId));
        }

        public string[] GetShadeThreads(string lockId)
        {
            return Search(GetShadeRow(lockId));
        }

        //public const string columnFamilyName = "lock";

        private string GetShadeRow(string rowName)
        {
            return "Shade_" + rowName;
        }

        private string GetMainRow(string rowName)
        {
            return "Main_" + rowName;
        }

        private long GetNowTicks()
        {
            var ticks = DateTime.UtcNow.Ticks;
            while(true)
            {
                var last = Interlocked.Read(ref lastTicks);
                var cur = Math.Max(ticks, last + 1);
                if(Interlocked.CompareExchange(ref lastTicks, cur, last) == last)
                    return cur;
            }
        }

        private void Delete(string rowName, string threadId)
        {
            MakeInConnection(connection => connection.DeleteBatch(rowName, new[] {threadId}, GetNowTicks()));
        }

        private void Write(string rowName, string threadId)
        {
            MakeInConnection(connection => connection.AddColumn(rowName, new Column
                {
                    Name = threadId,
                    Value = new byte[] {0},
                    Timestamp = GetNowTicks(),
                    TTL = 60
                }));
        }

        private string[] Search(string rowName)
        {
            var res = new string[0];
            MakeInConnection(connection =>
                {
                    var columns = connection.GetRow(rowName).ToArray();
                    if(columns.Length != 0)
                        res = columns.Where(x => x.Value != null && x.Value.Length != 0).Select(x => x.Name).ToArray();
                });
            return res;
        }

        private void MakeInConnection(Action<IColumnFamilyConnection> action)
        {
            var connection = cassandraCluster.RetrieveColumnFamilyConnection(keyspace, columnFamily);
            action(connection);
        }

        private readonly ICassandraCluster cassandraCluster;
        private readonly string keyspace;
        private readonly string columnFamily;

        private long lastTicks;
    }
}