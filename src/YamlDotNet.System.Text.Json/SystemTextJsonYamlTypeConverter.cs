using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Scalar = YamlDotNet.Core.Events.Scalar;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Allows YamlDotNet to de/serialize System.Text.Json objects
/// </summary>
public sealed class SystemTextJsonYamlTypeConverter : IYamlTypeConverter
{
    private bool SortKeysAlphabetically { get; }

    /// <summary>
    /// Allows YamlDotNet to de/serialize System.Text.Json objects
    /// </summary>
    /// <param name="sortKeysAlphabetically">sorts keys alphabetically when Serializing</param>
    public SystemTextJsonYamlTypeConverter(bool sortKeysAlphabetically = false)
    {
        SortKeysAlphabetically = sortKeysAlphabetically;
    }

    /// <summary>
    /// Determines whether the specified type is supported for JSON node processing.
    /// </summary>
    /// <remarks>This method is typically used to check if a type can be handled by APIs that operate on JSON
    /// nodes, elements, or documents. Passing a null value for the type parameter will result in an
    /// exception.</remarks>
    /// <param name="type">The type to evaluate for compatibility with JSON node operations. Must not be null.</param>
    /// <returns>true if the specified type is assignable from JsonNode, or is JsonElement or JsonDocument; otherwise, false.</returns>
    public bool Accepts(Type type)
    {
        return typeof(JsonNode).IsAssignableFrom(type)
            || type == typeof(JsonElement)
            || type == typeof(JsonDocument);
    }

    /// <summary>
    /// Deserializes YAML content from the specified parser into an object of the given type using the provided root
    /// deserializer.
    /// </summary>
    /// <remarks>Supported types include JsonValue, JsonArray, JsonObject, JsonNode, JsonDocument, and
    /// JsonElement. If the type is not one of these, the method returns null.</remarks>
    /// <param name="parser">The parser that reads the YAML input to be deserialized.</param>
    /// <param name="type">The target type to deserialize the YAML content into. Must be compatible with supported JSON node types.</param>
    /// <param name="rootDeserializer">The root deserializer used to convert YAML nodes into .NET objects.</param>
    /// <returns>An object representing the deserialized YAML content, or null if the specified type is not supported.</returns>
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (typeof(JsonValue).IsAssignableFrom(type))
        {
            return ReadJsonValue(parser);
        }
        else if (typeof(JsonArray).IsAssignableFrom(type))
        {
            return ReadJsonArray(parser, rootDeserializer);
        }
        else if (typeof(JsonObject).IsAssignableFrom(type))
        {
            return ReadJsonObject(parser, rootDeserializer);
        }
        else if (typeof(JsonNode).IsAssignableFrom(type))
        {
            return ReadJsonNode(parser, rootDeserializer);
        }
        else if (typeof(JsonDocument).IsAssignableFrom(type))
        {
            return ReadJsonDocument(parser, rootDeserializer);
        }
        else if (typeof(JsonElement).IsAssignableFrom(type))
        {
            return ReadJsonDocument(parser, rootDeserializer)?.RootElement;
        }

        return null;
    }

    /// <summary>
    /// Serializes the specified JSON value to YAML format using the provided emitter and serializer.
    /// </summary>
    /// <remarks>This method supports serializing various System.Text.Json types to YAML. The correct
    /// serialization logic is selected based on the runtime type of the value. The emitter must be valid and ready to
    /// receive YAML events.</remarks>
    /// <param name="emitter">The YAML emitter used to write the serialized output.</param>
    /// <param name="value">The JSON value to serialize. Can be a JsonValue, JsonArray, JsonObject, JsonNode, JsonDocument, or JsonElement.</param>
    /// <param name="type">The type of the value to serialize. Determines how the value is processed.</param>
    /// <param name="serializer">The object serializer used to convert values to YAML nodes.</param>
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (typeof(JsonValue).IsAssignableFrom(type))
        {
            WriteJsonValue(emitter!, value, serializer);
        }
        else if (typeof(JsonArray).IsAssignableFrom(type))
        {
            WriteJsonArray(emitter, value, serializer);
        }
        else if (typeof(JsonObject).IsAssignableFrom(type))
        {
            WriteJsonObject(emitter, value, serializer);
        }
        else if (typeof(JsonNode).IsAssignableFrom(type))
        {
            WriteJsonNode(emitter, value, serializer);
        }
        else if (typeof(JsonDocument).IsAssignableFrom(type))
        {
            WriteJsonDocument(emitter, value, serializer);
        }
        else if (typeof(JsonElement).IsAssignableFrom(type))
        {
            WriteJsonElement(emitter, value, serializer);
        }
    }

    // Read Functions

    private static JsonValue? ReadJsonValue(IParser parser)
    {
        if (parser.TryConsume<Scalar>(out var scalar))
        {
            if (scalar.Style == ScalarStyle.Plain)
            {
                if (long.TryParse(scalar.Value, out var i))
                {
                    return JsonValue.Create(i);
                }
                else if (double.TryParse(scalar.Value, out var d))
                {
                    return JsonValue.Create(d);
                }
                else if (bool.TryParse(scalar.Value, out var b))
                {
                    return JsonValue.Create(b);
                }
                else if (scalar.Value == "null")
                {
                    return null;
                }
            }

            // Always a string here
            return JsonValue.Create(scalar.Value);
        }

        return null;
    }

    private JsonArray? ReadJsonArray(IParser parser, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<SequenceStart>(out var start))
        {
            var array = new JsonArray();

            while (!parser.Accept<SequenceEnd>(out var end))
            {
                if (parser.Accept<Scalar>(out var scalar))
                {
                    array.Add(ReadYaml(parser, typeof(JsonValue), rootDeserializer) as JsonValue);
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    array.Add(ReadYaml(parser, typeof(JsonObject), rootDeserializer) as JsonObject);
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    array.Add(ReadYaml(parser, typeof(JsonArray), rootDeserializer) as JsonArray);
                }
            }

            parser.Consume<SequenceEnd>();

            return array;
        }

        return null;
    }

    private JsonObject? ReadJsonObject(IParser parser, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<MappingStart>(out var start))
        {
            var node = new JsonObject();

            while (!parser.Accept<MappingEnd>(out var end))
            {
                var name = parser.Consume<Scalar>();

                if (parser.Accept<Scalar>(out var scalar))
                {
                    node[name.Value] = ReadYaml(parser, typeof(JsonValue), rootDeserializer) as JsonValue;
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    node[name.Value] = ReadYaml(parser, typeof(JsonObject), rootDeserializer) as JsonObject;
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    node[name.Value] = ReadYaml(parser, typeof(JsonArray), rootDeserializer) as JsonArray;
                }
            }

            parser.Consume<MappingEnd>();

            return node;
        }

        return null;
    }

    private JsonNode? ReadJsonNode(IParser parser, ObjectDeserializer rootDeserializer)
    {
        var value = ReadJsonValue(parser);

        if (value != null)
        {
            return value;
        }

        var valueArray = ReadJsonArray(parser, rootDeserializer);

        if (valueArray != null)
        {
            return valueArray;
        }

        var jsonObject = ReadJsonObject(parser, rootDeserializer);

        if (jsonObject != null)
        {
            return jsonObject;
        }

        return null;
    }

    private JsonDocument? ReadJsonDocument(IParser parser, ObjectDeserializer rootDeserializer)
    {
        var readValue = ReadJsonValue(parser);

        if (readValue != null)
        {
            return JsonSerializer.SerializeToDocument(readValue);
        }

        var readArray = ReadJsonArray(parser, rootDeserializer);

        if (readArray != null)
        {
            return JsonSerializer.SerializeToDocument(readArray);
        }

        var readObject = ReadJsonObject(parser, rootDeserializer);

        if (readObject != null)
        {
            return JsonSerializer.SerializeToDocument(readObject);
        }

        return null;
    }

    // Write Functions

    private void WriteJsonValue(IEmitter emitter, object? value, ObjectSerializer serializer)
    {
        if (value is JsonValue val)
        {
            switch (val.GetValueKind())
            {
                case JsonValueKind.Undefined:
                    break;
                case JsonValueKind.Object:
                    WriteJsonObject(emitter, val, serializer);
                    break;
                case JsonValueKind.Array:
                    WriteJsonArray(emitter, value, serializer);
                    break;
                case JsonValueKind.String:
                    var valStr = val.ToString();

                    if (valStr.Contains('\n'))
                    {
                        // force it to be multi-line literal (aka |)
                        emitter.Emit(new Scalar(null, null, valStr, ScalarStyle.Literal, true, true));
                    }
                    else
                    {
                        // if string could be interpreted as a non-string value type, put quotes around it.
                        if (valStr == "null" ||
                            long.TryParse(valStr, out var _) ||
                            double.TryParse(valStr, out var _) ||
                            decimal.TryParse(valStr, out var _) ||
                            bool.TryParse(valStr, out var _))
                        {
                            emitter.Emit(new Scalar(null, null, valStr, ScalarStyle.SingleQuoted, true, true));
                        }
                        else
                        {
                            emitter.Emit(new Scalar(valStr));
                        }
                    }

                    break;
                case JsonValueKind.Number:
                    emitter.Emit(new Scalar(val.ToString()));
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
#pragma warning disable CA1308 // Normalize strings to uppercase
                    emitter.Emit(new Scalar(val.ToString().ToLowerInvariant()));
#pragma warning restore CA1308 // Normalize strings to uppercase
                    break;
                case JsonValueKind.Null:
                    emitter.Emit(new Scalar(null, "null"));
                    break;
            }
        }
    }

    private void WriteJsonArray(IEmitter emitter, object? value, ObjectSerializer serializer)
    {
        if (value is JsonArray jsonArray)
        {
            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Any));

            foreach (var item in jsonArray)
            {
                if (item == null)
                {
                    emitter.Emit(new Scalar(null, "null"));
                    continue;
                }

                WriteYaml(emitter, item, item.GetType(), serializer);
            }

            emitter.Emit(new SequenceEnd());
        }
    }

    private void WriteJsonObject(IEmitter emitter, object? value, ObjectSerializer serializer)
    {
        if (value is JsonObject jsonObject)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Any));

            foreach (var item in SortKeysAlphabetically ? jsonObject.OrderBy(x => x.Key).ToArray() : [.. jsonObject])
            {
                emitter.Emit(new Scalar(null, item.Key));

                if (item.Value == null)
                {
                    emitter.Emit(new Scalar(null, "null"));
                    continue;
                }

                WriteYaml(emitter, item.Value, item.Value.GetType(), serializer);
            }

            emitter.Emit(new MappingEnd());
        }
    }

    private void WriteJsonNode(IEmitter emitter, object? value, ObjectSerializer serializer)
    {
        if (value is JsonNode jsonNode)
        {
            switch (jsonNode.GetValueKind())
            {
                case JsonValueKind.Object:
                    WriteJsonObject(emitter, jsonNode.AsObject(), serializer);
                    break;
                case JsonValueKind.Array:
                    WriteJsonArray(emitter, jsonNode.AsArray(), serializer);
                    break;
                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    WriteJsonValue(emitter, jsonNode.AsValue(), serializer);
                    break;
                case JsonValueKind.Undefined:
                    break;
            }
        }
    }

    private void WriteJsonDocument(IEmitter emitter, object? value, ObjectSerializer serializer)
    {
        if (value is JsonDocument document)
        {
            WriteJsonElement(emitter, document.RootElement, serializer);
        }
    }

    private void WriteJsonElement(IEmitter emitter, object? value, ObjectSerializer serializer)
    {
        if (value is JsonElement jsonElement)
        {
            switch (jsonElement.ValueKind)
            {
                case JsonValueKind.Object:
                    WriteJsonObject(emitter, JsonObject.Create(jsonElement), serializer);
                    break;
                case JsonValueKind.Array:
                    WriteJsonArray(emitter, JsonArray.Create(jsonElement), serializer);
                    break;
                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    WriteJsonValue(emitter, JsonValue.Create(jsonElement), serializer);
                    break;
                case JsonValueKind.Undefined:
                    break;
            }
        }
    }
}
