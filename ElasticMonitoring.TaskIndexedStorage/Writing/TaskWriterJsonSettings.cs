using Newtonsoft.Json;

namespace SKBKontur.Catalogue.RemoteTaskQueue.ElasticMonitoring.TaskIndexedStorage.Writing
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
                            //new LongStringsToNullConverter2(),
                            new LongStringsToNullConverter(500),
                        }
                };
        }
    }
}