using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

// need type like {"PendingTaskCountsByNameAndTopic":{"(key1, key2)": object}}
public class TwoKeysDictionaryConvertor<T> : JsonConverter<Dictionary<(string, string), T>>
{
    public override Dictionary<(string, string), T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var intermediateDictionary = JsonSerializer.Deserialize<Dictionary<string, T>>(ref reader);
        var actualDictionary = new Dictionary<(string, string), T>();

        foreach (var keyValue in intermediateDictionary)
        {
            // stringKeys = "key1, key2"
            var stringKey = keyValue.Key.Substring(1, keyValue.Key.Length - 2);
            var stringKeys = stringKey.Split(',', ' ');
            var firstKey = stringKeys.First();
            var secondKey = stringKeys.Last();

            actualDictionary.Add((firstKey, secondKey), keyValue.Value);
        }

        return actualDictionary;
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<(string, string), T> value, JsonSerializerOptions options)
    {
        var convertedDictionary = new Dictionary<string, T>();
        foreach (var keyValue in value)
        {
            convertedDictionary.Add(keyValue.Key.ToString(), keyValue.Value);
        }
        JsonSerializer.Serialize(writer, convertedDictionary);
    }
}