using System;
using System.Collections.Concurrent;
using System.Linq;

using JetBrains.Annotations;

using SKBKontur.Catalogue.Objects;

namespace RemoteQueue.Cassandra.Entities
{
    public static class EnumExtensions
    {
        [NotNull]
        public static string GetCassandraName(this Enum value)
        {
            return cassandraNames.GetOrAdd(value, DoGetCassandraName);
        }

        [NotNull]
        private static string DoGetCassandraName(Enum value)
        {
            var memberInfos = value.GetType().GetMember(value.ToString());
            if(memberInfos.Any())
            {
                var attrs = memberInfos.Single().GetCustomAttributes(typeof(CassandraNameAttribute), false);
                if(attrs.Any())
                    return ((CassandraNameAttribute)attrs.Single()).Name;
            }
            throw new InvalidProgramStateException(string.Format("Не найдено значение CassandraNameAttribute для '{0}'", value));
        }

        private static readonly ConcurrentDictionary<Enum, string> cassandraNames = new ConcurrentDictionary<Enum, string>();
    }
}