﻿using System.Text.Json;
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
            || typeof(JsonArray).IsAssignableFrom(type)
            || typeof(JsonObject).IsAssignableFrom(type)
            || typeof(JsonValue).IsAssignableFrom(type)
            || typeof(JsonElement).IsAssignableFrom(type)
            || typeof(JsonDocument).IsAssignableFrom(type);
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
        else if (typeof(JsonObject).IsAssignableFrom(type) || typeof(JsonNode).IsAssignableFrom(type))
        {
            return ReadJsonObject(parser, rootDeserializer);
        }
        else if (typeof(JsonElement).IsAssignableFrom(type))
        {
            return ReadJsonDocument(parser, rootDeserializer).RootElement;
        }
        else if (typeof(JsonDocument).IsAssignableFrom(type))
        {
            return ReadJsonDocument(parser, rootDeserializer);
        }

        return null;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (typeof(JsonValue).IsAssignableFrom(type))
        {
            WriteJsonElement(emitter, value);
        }
        else if (typeof(JsonObject).IsAssignableFrom(type))
        {
            WriteJsonObject(emitter, value, serializer);
        }
        else if (typeof(JsonArray).IsAssignableFrom(type))
        {
            WriteJsonArray(emitter, value, serializer);
        }
        else if (typeof(JsonElement).IsAssignableFrom(type))
        {
            WriteJsonElement(emitter, value);
        }
        else if (typeof(JsonDocument).IsAssignableFrom(type))
        {
            WriteJsonDocument(emitter, value);
        }
    }

    // Read Functions

    private object ReadJsonValue(IParser parser)
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

    private object ReadJsonObject(IParser parser, ObjectDeserializer rootDeserializer)
    {
        var value = ReadJsonValue(parser);

        if (value != null)
        {
            return value;
        }

        var node = new JsonObject();

        if (parser.TryConsume<MappingStart>(out var start))
        {
            while (!parser.Accept<MappingEnd>(out var end))
            {
                var name = parser.Consume<Scalar>();

                if (parser.Accept<Scalar>(out var scalar))
                {
                    node[name.Value] = (JsonValue)ReadYaml(parser, typeof(JsonValue), rootDeserializer);
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    node[name.Value] = (JsonObject)ReadYaml(parser, typeof(JsonObject), rootDeserializer);
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    node[name.Value] = (JsonArray)ReadYaml(parser, typeof(JsonArray), rootDeserializer);
                }
            }

            parser.Consume<MappingEnd>();
        }

        return node;
    }

    private object ReadJsonArray(IParser parser, ObjectDeserializer rootDeserializer)
    {
        var array = new JsonArray();

        if (parser.TryConsume<SequenceStart>(out var start))
        {
            while (!parser.Accept<SequenceEnd>(out var end))
            {
                if (parser.Accept<Scalar>(out var scalar))
                {
                    array.Add((JsonValue)ReadYaml(parser, typeof(JsonValue), rootDeserializer));
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    array.Add((JsonObject)ReadYaml(parser, typeof(JsonObject), rootDeserializer));
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    array.Add((JsonArray)ReadYaml(parser, typeof(JsonArray), rootDeserializer));
                }
            }

            parser.Consume<SequenceEnd>();
        }

        return array;
    }

    private JsonDocument ReadJsonDocument(IParser parser, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<MappingStart>(out var start))
        {
            var node = new JsonObject();

            while (!parser.Accept<MappingEnd>(out var end))
            {
                var name = parser.Consume<Scalar>();

                if (parser.Accept<Scalar>(out var sc))
                {
                    node[name.Value] = (JsonValue)ReadYaml(parser, typeof(JsonValue), rootDeserializer);
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    node[name.Value] = (JsonObject)ReadYaml(parser, typeof(JsonObject), rootDeserializer);
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    node[name.Value] = (JsonArray)ReadYaml(parser, typeof(JsonArray), rootDeserializer);
                }
            }

            parser.Consume<MappingEnd>();

            return JsonSerializer.Deserialize<JsonDocument>(node);
        }

        if (parser.TryConsume<SequenceStart>(out var start2))
        {
            var array = new JsonArray();

            while (!parser.Accept<SequenceEnd>(out var end))
            {
                if (parser.Accept<Scalar>(out var scalar2))
                {
                    array.Add((JsonValue)ReadYaml(parser, typeof(JsonValue), rootDeserializer));
                }
                else if (parser.Accept<MappingStart>(out var mapStart))
                {
                    array.Add((JsonObject)ReadYaml(parser, typeof(JsonObject), rootDeserializer));
                }
                else if (parser.Accept<SequenceStart>(out var seqStart))
                {
                    array.Add((JsonArray)ReadYaml(parser, typeof(JsonArray), rootDeserializer));
                }
            }

            parser.Consume<SequenceEnd>();

            return JsonSerializer.SerializeToDocument(array);
        }

        if (parser.TryConsume<Scalar>(out var scalar))
        {
            if (scalar.Style == ScalarStyle.Plain)
            {
                if (long.TryParse(scalar.Value, out var i))
                {
                    return JsonSerializer.SerializeToDocument(i);
                }
                else if (double.TryParse(scalar.Value, out var d))
                {
                    return JsonSerializer.SerializeToDocument(d);
                }
                else if (bool.TryParse(scalar.Value, out var b))
                {
                    return JsonSerializer.SerializeToDocument(b);
                }
                else if (scalar.Value == "null")
                {
                    return JsonSerializer.SerializeToDocument((object)null);
                }
                else if (scalar.Value.GetType() == typeof(string))
                {
                    return JsonSerializer.SerializeToDocument(scalar.Value);
                }
            }
            else
            {
                return JsonSerializer.SerializeToDocument(scalar.Value);
            }
        }

        return null;
    }

    // Write Functions

    private void WriteJsonObject(IEmitter emitter, object value, ObjectSerializer serializer)
    {
        emitter.Emit(new MappingStart(null, null, false, MappingStyle.Any));

        foreach (var property in SortKeysAlphabetically ? ((JsonObject)value).OrderBy(x => x.Key).ToArray() : ((JsonObject)value).ToArray())
        {
            JsonNode propVal = property.Value;

            emitter.Emit(new Scalar(null, property.Key));

            if (property.Value == null)
            {
                WriteJsonElement(emitter, null);
            }
            else
            {
                WriteYaml(emitter, propVal, propVal.GetType(), serializer);
            }
        }

        emitter.Emit(new MappingEnd());
    }

    private void WriteJsonDocument(IEmitter emitter, object value)
    {
        var doc = (JsonDocument)value;

        var root = doc.RootElement;

        WriteJsonElement(emitter, root);
    }

    private void WriteJsonElement(IEmitter emitter, object value)
    {
        if (value == null)
        {
            emitter.Emit(new Scalar(null, "null"));
            return;
        }

        JsonElement obj;

        if (typeof(JsonElement).IsAssignableFrom(value.GetType()))
        {
            obj = (JsonElement)value;
        }
        else
        {
            obj = ((JsonValue)value).Deserialize<JsonElement>();
        }

        switch (obj.ValueKind)
        {
            case JsonValueKind.Object:
                emitter.Emit(new MappingStart(null, null, false, MappingStyle.Any));

                foreach (var item in SortKeysAlphabetically ? obj.EnumerateObject().OrderBy(x => x.Name).ToArray() : obj.EnumerateObject().ToArray())
                {
                    emitter.Emit(new Scalar(null, item.Name));

                    WriteJsonElement(emitter, item.Value);
                }
                emitter.Emit(new MappingEnd());

                break;
            case JsonValueKind.Array:
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Any));

                foreach (var item in obj.EnumerateArray())
                {
                    WriteJsonElement(emitter, item);
                }

                emitter.Emit(new SequenceEnd());

                break;
            case JsonValueKind.String:
                var val = obj.ToString();

                if (val.IndexOf("\n") > 0)
                {
                    // force it to be multi-line literal (aka |)
                    emitter.Emit(new Scalar(null, null, val, ScalarStyle.Literal, true, true));
                }
                else
                {
                    // if string could be interpreted as a non-string value type, put quotes around it.
                    if (val == "null" ||
                        long.TryParse(val, out var _) ||
                        double.TryParse(val, out var _) ||
                        decimal.TryParse(val, out var _) ||
                        bool.TryParse(val, out var _))
                    {
                        emitter.Emit(new Scalar(null, null, val, ScalarStyle.SingleQuoted, true, true));
                    }
                    else
                    {
                        emitter.Emit(new Scalar(val));
                    }
                }
                break;
            case JsonValueKind.Number:
                emitter.Emit(new Scalar(obj.ToString()));
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                emitter.Emit(new Scalar(obj.ToString().ToLower()));
                break;
            case JsonValueKind.Null:
                emitter.Emit(new Scalar(null, "null"));
                break;
        }
    }

    private void WriteJsonArray(IEmitter emitter, object value, ObjectSerializer serializer)
    {
        var style = SequenceStyle.Any;

        emitter.Emit(new SequenceStart(null, null, false, style));

        foreach (var item in ((JsonArray)value))
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
