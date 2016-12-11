using Newtonsoft.Json;

namespace RemoteTaskQueue.Monitoring.Storage.Writing
{
    public class TaskWriterJsonSettings
    {
        public static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
                {
                    ContractResolver = new OmitNonIndexablePropertiesContractResolver(),
                    Converters = new JsonConverter[]
                        {
                            new TruncateLongStringsConverter(500),
                        }
                };
        }
    }
}