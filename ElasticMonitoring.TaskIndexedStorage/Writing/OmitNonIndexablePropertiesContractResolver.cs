using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
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
                    if(elementType.IsAbstract || elementType.IsInterface || elementType == typeof(object) || elementType == typeof(byte[]))
                        return null;
                }
                if(propertyInfo.PropertyType.IsAbstract || propertyInfo.PropertyType.IsInterface || propertyInfo.PropertyType == typeof(object) || propertyInfo.PropertyType == typeof(byte[]))
                    return null;
            }
            return base.CreateProperty(member, memberSerialization);
        }
    }
}