using System;
using System.CodeDom;
using System.Dynamic;
using System.Linq;

using RemoteQueue.Cassandra.Primitives;

using SKBKontur.Cassandra.CassandraClient.Abstractions;

namespace ExchangeService.UserClasses
{
    public interface ITestTaskLogger
    {
        void Log(string loggingTaskIdKey, string taskId);
        string[] GetAll(string loggingTaskIdKey);
    }

    public class CassandraTestTaskLogger : ColumnFamilyRepositoryBase, ITestTaskLogger
    {
        public CassandraTestTaskLogger(IColumnFamilyRepositoryParameters parameters) :
            base(parameters, columnFamilyName)
        {
        }

        public void Log(string loggingTaskIdKey, string taskId)
        {
            RetrieveColumnFamilyConnection().AddColumn(loggingTaskIdKey, new Column
                {
                    Name = taskId,
                    Value = new byte[]{1},
                    Timestamp = DateTime.UtcNow.Ticks
                });
        }

        public string[] GetAll(string loggingTaskIdKey)
        {
            return RetrieveColumnFamilyConnection().GetRow(loggingTaskIdKey).Select(column => column.Name).ToArray();
        }

        public const string columnFamilyName = "CassandraTestTaskLogger";
    }
}