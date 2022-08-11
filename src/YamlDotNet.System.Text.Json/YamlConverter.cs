﻿using YamlDotNet.Serialization;

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

        public static string Serialize(object obj, ISerializer? serializer = null)
        {
            serializer ??= DefaultSerializer;
            return serializer.Serialize(obj);
        }

        public static T Deserialize<T>(string yaml, IDeserializer? deserializer = null)
        {
            deserializer ??= DefaultDeserializer;
            return deserializer.Deserialize<T>(yaml);
        }
    }
}