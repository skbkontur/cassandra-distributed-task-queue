﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.Json;

public class OmitBinaryAndAbstractPropertyConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return IsAbstractType(typeToConvert) || IsBinaryType(typeToConvert) || base.CanConvert(typeToConvert);
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonSerializer.Deserialize(ref reader, typeToConvert);
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        writer.WriteNullValue();
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