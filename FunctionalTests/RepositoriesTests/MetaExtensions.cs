using System;

using RemoteQueue.Cassandra.Entities;

namespace FunctionalTests.RepositoriesTests
{
    public static class MetaExtensions
    {
        public static TaskMetaInformation ExpiredAfter(this TaskMetaInformation meta, TimeSpan ttl)
        {
            meta.SetUpExpiration(ttl);
            return meta;
        }
    }
}