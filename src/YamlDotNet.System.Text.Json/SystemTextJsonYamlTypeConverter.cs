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

    public bool Accepts(Type type)
    {
        return typeof(JsonNode).IsAssignableFrom(type)
            || type == typeof(JsonElement)
            || type == typeof(JsonDocument);
    }

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

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (typeof(JsonValue).IsAssignableFrom(type))
        {
            WriteJsonValue(emitter, value, serializer);
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
                    return JsonValue.Create((object)null);
                }
                else if (scalar.Value.GetType() == typeof(string))
                {
                    return JsonValue.Create(scalar.Value);
                }
            }
            else
            {
                return JsonValue.Create(scalar.Value);
            }
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

                    if (valStr.IndexOf('\n') > 0)
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
                    emitter.Emit(new Scalar(val.ToString().ToLower()));
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
