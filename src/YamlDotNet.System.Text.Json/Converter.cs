using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json
{
    public static class YamlConverter
    {
        public static ISerializer DefaultSerializer = new SerializerBuilder()
                .DisableAliases()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitDefaults)
                .WithTypeConverter(new JsonNodeYamlConverter())
                .Build();

        public static IDeserializer DefaultDeserializer = new DeserializerBuilder()
                .WithTypeConverter(new JsonNodeYamlConverter())
                .Build();

        public static string Serialize(JsonNode jsonNode, ISerializer? serializer = null)
        {
            serializer ??= DefaultSerializer;
            return serializer.Serialize(jsonNode);
        }

        public static JsonNode Deserialize(string yaml, IDeserializer? deserializer = null)
        {
            deserializer ??= DefaultDeserializer;
            return deserializer.Deserialize<JsonNode>(yaml);
        }
    }
}
