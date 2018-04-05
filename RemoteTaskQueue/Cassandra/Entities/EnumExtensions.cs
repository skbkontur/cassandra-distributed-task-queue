using System;
using System.Collections;
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
            return cassandraNames.GetOrAddThreadSafely(value, DoGetCassandraName);
        }

        [NotNull]
        private static string DoGetCassandraName(Enum value)
        {
            var memberInfos = value.GetType().GetMember(value.ToString());
            if (memberInfos.Any())
            {
                var attrs = memberInfos.Single().GetCustomAttributes(typeof(CassandraNameAttribute), false);
                if (attrs.Any())
                    return ((CassandraNameAttribute)attrs.Single()).Name;
            }
            throw new InvalidProgramStateException($"Не найдено значение CassandraNameAttribute для '{value}'");
        }

        private static readonly Hashtable cassandraNames = new Hashtable();
    }
}