using System.Text.Json;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

public static class YamlConverter
{
    public static ISerializer DefaultSerializer = new SerializerBuilder()
            .DisableAliases()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .Build();

    public static IDeserializer DefaultDeserializer = new DeserializerBuilder()
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .Build();

    public static string Serialize(object obj, ISerializer? serializer = null)
    {
        serializer ??= DefaultSerializer;
        return serializer.Serialize(obj);
    }

    public static string SerializeJson(string json, ISerializer? serializer = null, JsonSerializerOptions jsonSerializerOptions = null)
    {
        serializer ??= DefaultSerializer;
        return serializer.Serialize(JsonSerializer.Deserialize<JsonDocument>(json, jsonSerializerOptions));
    }

    public static T Deserialize<T>(string yaml, IDeserializer? deserializer = null)
    {
        deserializer ??= DefaultDeserializer;
        return deserializer.Deserialize<T>(yaml);
    }
}
