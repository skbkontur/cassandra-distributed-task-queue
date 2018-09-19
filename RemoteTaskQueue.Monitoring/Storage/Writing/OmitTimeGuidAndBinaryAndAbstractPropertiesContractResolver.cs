using System.Reflection;

using log4net;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RemoteTaskQueue.Monitoring.Storage.Utils;

using SKBKontur.Catalogue.Objects.TimeBasedUuid;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class OmitTimeGuidAndBinaryAndAbstractPropertiesContractResolver : OmitBinaryAndAbstractPropertiesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType.IsArray)
                {
                    var elementType = propertyInfo.PropertyType.GetElementType();
                    if (elementType == typeof(TimeGuid))
                    {
                        logger.LogInfoFormat("Omitting TimeGuid property {0} for type {1}", propertyInfo.Name, propertyInfo.DeclaringType?.FullName);
                        return null;
                    }
                }
                if (propertyInfo.PropertyType == typeof(TimeGuid))
                {
                    logger.LogInfoFormat("Omitting TimeGuid property {0} for type {1}", propertyInfo.Name, propertyInfo.DeclaringType?.FullName);
                    return null;
                }
            }
            return base.CreateProperty(member, memberSerialization);
        }

        private static readonly ILog logger = LogManager.GetLogger("TimeGuidPropertyOmitter");
    }
}