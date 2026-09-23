using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
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
    [RequiresUnreferencedCode("YamlDotNet's reflection based serializer is not trim safe.")]
    private static ISerializer GetSerializer(bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        return new SerializerBuilder()
            .ConfigureDefaultValuesHandling(defaultValuesHandling)
            .AddSystemTextJson(sortAlphabetically, ignoreOrder)
            .Build();
    }

    [RequiresUnreferencedCode("YamlDotNet's reflection based deserializer is not trim safe.")]
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
    [RequiresUnreferencedCode("YamlDotNet's default serializer uses runtime reflection. Use the overload that accepts a generated StaticContext for trimming support.")]
    public static string Serialize(object obj, bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        return SerializeCore(obj, sortAlphabetically, ignoreOrder, defaultValuesHandling);
    }

    /// <summary>
    /// Serializes an object with a statically known type to a string.
    /// </summary>
    /// <typeparam name="T">The statically known type to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="sortAlphabetically">Whether to sort properties alphabetically.</param>
    /// <param name="ignoreOrder">Whether to ignore <see cref="JsonPropertyOrderAttribute"/>.</param>
    /// <param name="defaultValuesHandling">How default values are handled.</param>
    /// <returns>The serialized YAML.</returns>
    [RequiresUnreferencedCode("YamlDotNet's default serializer uses runtime reflection. Use the overload that accepts a generated StaticContext for trimming support.")]
    public static string Serialize<T>(T obj, bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        return SerializeCore(obj, sortAlphabetically, ignoreOrder, defaultValuesHandling);
    }

    /// <summary>
    /// Serializes an object using a generated YamlDotNet static context.
    /// </summary>
    /// <typeparam name="T">The registered type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="context">The generated context containing the type metadata.</param>
    /// <param name="sortAlphabetically">Whether to sort keys in JSON objects.</param>
    /// <param name="ignoreOrder">Whether to ignore <see cref="JsonPropertyOrderAttribute"/> on POCO properties.</param>
    /// <param name="defaultValuesHandling">How default values are handled during serialization.</param>
    /// <returns>The serialized YAML.</returns>
    public static string Serialize<T>(T obj, StaticContext context, bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
#if NETSTANDARD2_0
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }
#else
        ArgumentNullException.ThrowIfNull(context);
#endif
        var serializer = new StaticSerializerBuilder(context)
            .ConfigureDefaultValuesHandling(defaultValuesHandling)
            .AddSystemTextJson(sortAlphabetically, ignoreOrder)
            .Build();
        return serializer.Serialize(obj);
    }

    [RequiresUnreferencedCode("YamlDotNet's reflection based serializer is not trim safe.")]
    private static string SerializeCore(object? obj, bool sortAlphabetically, bool ignoreOrder, DefaultValuesHandling defaultValuesHandling)
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
    [RequiresUnreferencedCode("YamlDotNet's default serializer uses runtime reflection. Use YamlDotNet's static builder APIs with a generated StaticContext for trimming support.")]
    public static string SerializeJson(string json, JsonSerializerOptions? jsonSerializerOptions = null, bool sortAlphabetically = false, bool ignoreOrder = false, DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        var serializer = GetSerializer(sortAlphabetically, ignoreOrder, defaultValuesHandling);
        var documentOptions = jsonSerializerOptions is null
            ? default
            : new JsonDocumentOptions
            {
                AllowTrailingCommas = jsonSerializerOptions.AllowTrailingCommas,
                CommentHandling = jsonSerializerOptions.ReadCommentHandling,
                MaxDepth = jsonSerializerOptions.MaxDepth,
            };

        return serializer.Serialize(JsonDocument.Parse(json, documentOptions));
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
    [RequiresUnreferencedCode("YamlDotNet's default deserializer uses runtime reflection. Use the overload that accepts a generated StaticContext for trimming support.")]
    public static T Deserialize<T>(string yaml, bool ignoreUnmatchedProperties = false)
    {
        var deserializer = GetDeserializer(ignoreUnmatchedProperties);
        return deserializer.Deserialize<T>(yaml);
    }

    /// <summary>
    /// Deserializes YAML to a statically registered type using a generated YamlDotNet static context.
    /// </summary>
    /// <typeparam name="T">The registered type to deserialize.</typeparam>
    /// <param name="yaml">The YAML text to deserialize.</param>
    /// <param name="context">The generated context containing the type metadata.</param>
    /// <param name="ignoreUnmatchedProperties">Whether to ignore YAML properties not present on the registered type.</param>
    /// <returns>The deserialized value.</returns>
    public static T Deserialize<T>(string yaml, StaticContext context, bool ignoreUnmatchedProperties = false)
    {
#if NETSTANDARD2_0
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }
#else
        ArgumentNullException.ThrowIfNull(context);
#endif
        var builder = new StaticDeserializerBuilder(context)
            .AddSystemTextJson();

        if (ignoreUnmatchedProperties)
        {
            builder.IgnoreUnmatchedProperties();
        }

        return builder.Build().Deserialize<T>(yaml);
    }
}
