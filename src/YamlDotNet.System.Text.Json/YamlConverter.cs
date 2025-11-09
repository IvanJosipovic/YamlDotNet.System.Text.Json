using System.Text.Json;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

public static class YamlConverter
{
    private static ISerializer GetSerializer(bool sortAlphabetically = false, bool ignoreOrder = false)
    {
        return new SerializerBuilder()
            .DisableAliases()
            //.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .AddSystemTextJson(sortAlphabetically, ignoreOrder)
            .Build();
    }

    private static IDeserializer GetDeserializer()
    {
        return new DeserializerBuilder()
            .AddSystemTextJson()
            .Build();
    }

    public static string Serialize(object obj, ISerializer? serializer = null, bool sortAlphabetically = false, bool ignoreOrder = false)
    {
        serializer ??= GetSerializer(sortAlphabetically, ignoreOrder);
        return serializer.Serialize(obj);
    }

    public static string SerializeJson(string json, ISerializer? serializer = null, JsonSerializerOptions? jsonSerializerOptions = null, bool sortAlphabetically = false, bool ignoreOrder = false)
    {
        serializer ??= GetSerializer(sortAlphabetically, ignoreOrder);
        return serializer.Serialize(JsonSerializer.Deserialize<JsonDocument>(json, jsonSerializerOptions));
    }

    public static T Deserialize<T>(string yaml, IDeserializer? deserializer = null)
    {
        deserializer ??= GetDeserializer();
        return deserializer.Deserialize<T>(yaml);
    }
}
