using System;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    internal class OmitNonIndexablePropertiesContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var propertyInfo = member as PropertyInfo;
            if(propertyInfo != null)
            {
                if(propertyInfo.PropertyType.IsArray)
                {
                    var elementType = propertyInfo.PropertyType.GetElementType();
                    if(IsBadType(elementType))
                        return null;
                }
                if(IsBadType(propertyInfo.PropertyType))
                    return null;
            }
            return base.CreateProperty(member, memberSerialization);
        }

        private static bool IsBadType(Type elementType)
        {
            return elementType.IsAbstract || elementType.IsInterface || elementType == typeof(object) || elementType == typeof(byte[]);
        }
    }
}