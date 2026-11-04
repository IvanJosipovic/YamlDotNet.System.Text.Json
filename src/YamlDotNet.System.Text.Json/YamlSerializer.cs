using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using YamlDotNet.Core;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Serializes and deserializes YAML using System.Text.Json source generated contracts.
/// </summary>
/// <remarks>
/// The <see cref="YamlSerializer"/> reuses the <see cref="JsonTypeInfo{T}"/> metadata produced by
/// <see cref="JsonSerializerContext"/> source generation, so the
/// same contracts, converters, attributes and options are used for both JSON and YAML. The YAML is
/// converted to and from a JSON token stream and the actual object mapping is delegated to
/// <see cref="JsonSerializer"/>, which keeps reflection out of the picture and makes the API
/// compatible with Native AOT and trimming.
/// <para>
/// A JSON compatible subset of YAML is supported; see the project README for the supported scalar
/// semantics and unsupported features.
/// </para>
/// All methods are thread safe and do not maintain any internal state.
/// </remarks>
/// <example>
/// <code>
/// [JsonSerializable(typeof(Person))]
/// internal partial class AppJsonContext : JsonSerializerContext;
///
/// var person = new Person { Name = "John", Age = 30 };
/// string yaml = YamlSerializer.Serialize(person, AppJsonContext.Default.Person);
/// Person? result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.Person);
/// </code>
/// </example>
public static class YamlSerializer
{
    /// <summary>
    /// Serializes the specified value to a YAML string using the provided System.Text.Json contract.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize.</param>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> contract, typically obtained from a source generated <see cref="JsonSerializerContext"/>.</param>
    /// <returns>A YAML representation of the value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsonTypeInfo"/> is null.</exception>
    public static string Serialize<T>(T value, JsonTypeInfo<T> jsonTypeInfo)
    {
#if NETSTANDARD2_0
        if (jsonTypeInfo is null)
        {
            throw new ArgumentNullException(nameof(jsonTypeInfo));
        }
#else
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
#endif
        var buffer = new PooledBufferWriter();

        try
        {
            using (var writer = new Utf8JsonWriter(buffer))
            {
                JsonSerializer.Serialize(writer, value, jsonTypeInfo);
            }

            return JsonToYamlWriter.ConvertToString(buffer.WrittenSpan);
        }
        finally
        {
            buffer.Dispose();
        }
    }

    /// <summary>
    /// Serializes the specified value as YAML to the provided stream using the provided System.Text.Json contract.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="stream">The stream to write YAML to. The stream is not closed or disposed.</param>
    /// <param name="value">The value to serialize.</param>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> contract, typically obtained from a source generated <see cref="JsonSerializerContext"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
    public static void Serialize<T>(Stream stream, T value, JsonTypeInfo<T> jsonTypeInfo)
    {
#if NETSTANDARD2_0
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (jsonTypeInfo is null)
        {
            throw new ArgumentNullException(nameof(jsonTypeInfo));
        }
#else
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
#endif
        var buffer = new PooledBufferWriter();

        try
        {
            using (var writer = new Utf8JsonWriter(buffer))
            {
                JsonSerializer.Serialize(writer, value, jsonTypeInfo);
            }

            using var streamWriter = new StreamWriter(stream, JsonToYamlWriter.Utf8NoBom, bufferSize: 1024, leaveOpen: true);
            JsonToYamlWriter.Convert(buffer.WrittenSpan, streamWriter);
        }
        finally
        {
            buffer.Dispose();
        }
    }

    /// <summary>
    /// Deserializes a YAML string into an instance of type <typeparamref name="T"/> using the provided System.Text.Json contract.
    /// </summary>
    /// <typeparam name="T">The type of the value to deserialize.</typeparam>
    /// <param name="yaml">The YAML string to deserialize.</param>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> contract, typically obtained from a source generated <see cref="JsonSerializerContext"/>.</param>
    /// <returns>An instance of <typeparamref name="T"/> populated from the YAML input.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="yaml"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
    /// <exception cref="YamlDotNet.Core.YamlException">Thrown when the YAML is invalid or uses unsupported YAML features.</exception>
    public static T? Deserialize<T>(string yaml, JsonTypeInfo<T> jsonTypeInfo)
    {
#if NETSTANDARD2_0
        if (yaml is null)
        {
            throw new ArgumentNullException(nameof(yaml));
        }
        if (jsonTypeInfo is null)
        {
            throw new ArgumentNullException(nameof(jsonTypeInfo));
        }
#else
        ArgumentNullException.ThrowIfNull(yaml);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
#endif
        var parser = new Parser(new StringReader(yaml));

        return DeserializeCore(parser, jsonTypeInfo);
    }

    /// <summary>
    /// Deserializes YAML from the provided stream into an instance of type <typeparamref name="T"/> using the provided System.Text.Json contract.
    /// </summary>
    /// <typeparam name="T">The type of the value to deserialize.</typeparam>
    /// <param name="stream">The stream to read YAML from. The stream is not closed or disposed.</param>
    /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> contract, typically obtained from a source generated <see cref="JsonSerializerContext"/>.</param>
    /// <returns>An instance of <typeparamref name="T"/> populated from the YAML input.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> or <paramref name="jsonTypeInfo"/> is null.</exception>
    /// <exception cref="YamlDotNet.Core.YamlException">Thrown when the YAML is invalid or uses unsupported YAML features.</exception>
    public static T? Deserialize<T>(Stream stream, JsonTypeInfo<T> jsonTypeInfo)
    {
#if NETSTANDARD2_0
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }
        if (jsonTypeInfo is null)
        {
            throw new ArgumentNullException(nameof(jsonTypeInfo));
        }
#else
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(jsonTypeInfo);
#endif
        var parser = new Parser(new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true));

        return DeserializeCore(parser, jsonTypeInfo);
    }

    private static T? DeserializeCore<T>(IParser parser, JsonTypeInfo<T> jsonTypeInfo)
    {
        var buffer = new PooledBufferWriter();

        try
        {
            using (var writer = new Utf8JsonWriter(buffer))
            {
                YamlToJsonWriter.Convert(parser, writer);
            }

            return JsonSerializer.Deserialize(buffer.WrittenSpan, jsonTypeInfo);
        }
        finally
        {
            buffer.Dispose();
        }
    }
}
