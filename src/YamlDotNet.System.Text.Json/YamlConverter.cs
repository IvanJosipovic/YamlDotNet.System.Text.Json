using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Provides static methods for serializing objects to YAML, deserializing YAML to objects, and converting JSON to YAML
/// using configurable serialization options.
/// </summary>
/// <remarks>The YamlConverter class offers convenience methods for working with YAML and JSON data formats. It
/// supports customization of serialization behavior, such as property ordering and handling of default values, by
/// adjusting options via method parameters.
/// All methods are thread-safe and do not maintain any internal state.</remarks>
public static class YamlConverter
{
    private static ISerializer GetSerializer(bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        return new SerializerBuilder()
            .ConfigureDefaultValuesHandling(defaultValuesHandling)
            .AddSystemTextJson(sortAlphabetically, ignoreOrder)
            .Build();
    }

    private static IDeserializer GetDeserializer(bool ignoreUnmatchedProperties = false)
    {
        var builder = new DeserializerBuilder()
            .AddSystemTextJson();

        if (ignoreUnmatchedProperties)
        {
            builder.IgnoreUnmatchedProperties();
        }

        return builder.Build();
    }

    /// <summary>
    /// Serializes the specified object to a string
    /// </summary>
    /// <param name="obj">The object to serialize. Can be any type supported by the serializer.</param>
    /// <param name="sortAlphabetically">Specifies whether object properties should be sorted alphabetically during serialization. Set to <see langword="true"/> to sort properties; otherwise, properties retain their original order.</param>
    /// <param name="ignoreOrder">Specifies whether <see cref="JsonPropertyOrderAttribute"/> should be ignored during type inspection. Set to <see langword="true"/> to ignore the attribute-defined order; otherwise, any order defined via <see cref="JsonPropertyOrderAttribute"/> is preserved.</param>
    /// <param name="defaultValuesHandling">Specifies how default values are handled during serialization. Determines whether default values are preserved
    /// or omitted.</param>
    /// <returns>A string containing the serialized representation of the object.</returns>
    public static string Serialize(object obj, bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        var serializer = GetSerializer(sortAlphabetically, ignoreOrder, defaultValuesHandling);
        return serializer.Serialize(obj);
    }

    /// <summary>
    /// Serializes the specified JSON string to YAML
    /// </summary>
    /// <remarks> The input JSON must be valid; otherwise, deserialization may fail.</remarks>
    /// <param name="json">The JSON string to serialize. Must be a valid JSON document.</param>
    /// <param name="jsonSerializerOptions">Optional settings to control JSON deserialization behavior. If null, default options are used.</param>
    /// <param name="sortAlphabetically">Specifies whether object properties should be sorted alphabetically during serialization. Set to <see langword="true"/> to sort properties; otherwise, properties retain their original order.</param>
    /// <param name="ignoreOrder">Specifies whether <see cref="JsonPropertyOrderAttribute"/> should be ignored during type inspection. Set to <see langword="true"/> to ignore the attribute-defined order; otherwise, any order defined via <see cref="JsonPropertyOrderAttribute"/> is preserved.</param>
    /// <param name="defaultValuesHandling">Specifies how default values are handled during serialization. Use DefaultValuesHandling.Preserve to retain
    /// default values, or other options to modify this behavior.</param>
    /// <returns>A string containing the serialized representation of the input JSON.</returns>
    public static string SerializeJson(string json, JsonSerializerOptions? jsonSerializerOptions = null, bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        var serializer = GetSerializer(sortAlphabetically, ignoreOrder, defaultValuesHandling);
        return serializer.Serialize(JsonSerializer.Deserialize<JsonDocument>(json, jsonSerializerOptions));
    }

    /// <summary>
    /// Deserializes a YAML string into an object of the specified type.
    /// </summary>
    /// <remarks>If no deserializer is provided, a default implementation is used. The method expects the YAML
    /// to be valid and compatible with the target type.</remarks>
    /// <typeparam name="T">The type of object to deserialize the YAML content into.</typeparam>
    /// <param name="yaml">The YAML string to deserialize. Cannot be null.</param>
    /// <param name="ignoreUnmatchedProperties">Instructs the deserializer to ignore unmatched properties instead of throwing an exception.</param>
    /// <returns>An instance of type T populated with data from the YAML string.</returns>
    public static T Deserialize<T>(string yaml, bool ignoreUnmatchedProperties = false)
    {
        var deserializer = GetDeserializer(ignoreUnmatchedProperties);
        return deserializer.Deserialize<T>(yaml);
    }
}
