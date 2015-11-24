using System;
using System.Reflection;

namespace RemoteQueue.Cassandra.Entities
{
    public static class TaskMetaInformationExtension
    {
        public static string GetCassandraName(this Enum value)
        {
            MemberInfo[] memberInfos = value.GetType().GetMember(value.ToString());
            if(memberInfos.Length > 0)
            {
                object[] attrs = memberInfos[0].GetCustomAttributes(typeof(CassandraNameAttribute), false);
                if(attrs.Length > 0)
                    return ((CassandraNameAttribute)attrs[0]).Name;
            }
            throw new Exception(string.Format("Не найдено значение CassandraNameAttribute для '{0}'", value));
        }
    }
}