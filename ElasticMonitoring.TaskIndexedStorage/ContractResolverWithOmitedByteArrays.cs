using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage
{
    internal class ContractResolverWithOmitedByteArrays : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var propertyInfo = member as PropertyInfo;
            if(propertyInfo != null)
            {
                if(propertyInfo.PropertyType == typeof(byte[]))
                    return null;
            }
            return base.CreateProperty(member, memberSerialization);
        }
    }
}