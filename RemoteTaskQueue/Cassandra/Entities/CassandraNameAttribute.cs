using System;

namespace RemoteQueue.Cassandra.Entities
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class CassandraNameAttribute : Attribute
    {
        public CassandraNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}