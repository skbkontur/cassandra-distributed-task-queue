using System;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class OmitBinaryAndAbstractPropertiesContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType.IsArray)
                {
                    var elementType = propertyInfo.PropertyType.GetElementType();
                    if (IsBinaryType(elementType) || IsAbstractType(elementType))
                        return null;
                }
                if (IsBinaryType(propertyInfo.PropertyType) || IsAbstractType(propertyInfo.PropertyType))
                    return null;
            }
            return base.CreateProperty(member, memberSerialization);
        }

        private static bool IsBinaryType(Type elementType)
        {
            return elementType == typeof(byte[]);
        }

        private static bool IsAbstractType(Type elementType)
        {
            return elementType.IsAbstract || elementType == typeof(object);
        }
    }
}