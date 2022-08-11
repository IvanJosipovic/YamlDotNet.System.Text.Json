using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using Scalar = YamlDotNet.Core.Events.Scalar;

namespace YamlDotNet.System.Text.Json
{
    public sealed class JsonNodeYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return typeof(JsonNode).IsAssignableFrom(type)
                || typeof(JsonArray).IsAssignableFrom(type)
                || typeof(JsonObject).IsAssignableFrom(type)
                || typeof(JsonValue).IsAssignableFrom(type);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            if (typeof(JsonValue).IsAssignableFrom(type))
            {
                return ReadValue(parser);
            }
            else if (typeof(JsonArray).IsAssignableFrom(type))
            {
                return ReadArray(parser);
            }
            else if (typeof(JsonObject).IsAssignableFrom(type) || typeof(JsonNode).IsAssignableFrom(type))
            {
                return ReadObject(parser);
            }
            else
            {
                //Shouldn't be here!
                throw new Exception("Unknown Type :" + type.FullName);
            }
        }

        private object ReadValue(IParser parser)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                if (scalar.Style == ScalarStyle.Plain)
                {
                    if (long.TryParse(scalar.Value, out var i))
                    {
                        return JsonValue.Create(i);
                    }
                    else if (float.TryParse(scalar.Value, out var f))
                    {
                        return JsonValue.Create(f);
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

        private object ReadObject(IParser parser)
        {
            var value = ReadValue(parser);

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
                        node[name.Value] = (JsonValue)ReadYaml(parser, typeof(JsonValue));
                    }
                    else if (parser.Accept<MappingStart>(out var mapStart))
                    {
                        node[name.Value] = (JsonObject)ReadYaml(parser, typeof(JsonObject));
                    }
                    else if (parser.Accept<SequenceStart>(out var seqStart))
                    {
                        node[name.Value] = (JsonArray)ReadYaml(parser, typeof(JsonArray));
                    }
                }

                parser.Consume<MappingEnd>();
            }

            return node;
        }

        private object ReadArray(IParser parser)
        {
            var array = new JsonArray();

            if (parser.TryConsume<SequenceStart>(out var start))
            {
                while (!parser.Accept<SequenceEnd>(out var end))
                {
                    if (parser.Accept<Scalar>(out var scalar))
                    {
                        array.Add((JsonValue)ReadYaml(parser, typeof(JsonValue)));
                    }
                    else if (parser.Accept<MappingStart>(out var mapStart))
                    {
                        array.Add((JsonObject)ReadYaml(parser, typeof(JsonObject)));
                    }
                    else if (parser.Accept<SequenceStart>(out var seqStart))
                    {
                        array.Add((JsonArray)ReadYaml(parser, typeof(JsonArray)));
                    }
                }

                parser.Consume<SequenceEnd>();
            }

            return array;
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (typeof(JsonValue).IsAssignableFrom(type))
            {
                WriteValue(emitter, value);
            }
            else if (typeof(JsonObject).IsAssignableFrom(type))
            {
                WriteObject(emitter, value);
            }
            else if (typeof(JsonArray).IsAssignableFrom(type))
            {
                WriteArray(emitter, value);
            }
            else
            {
                //Shouldn't be here!
                throw new Exception("Unknown Type :" + type.FullName);
            }
        }

        private void WriteObject(IEmitter emitter, object value)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Any));

            foreach (var property in (JsonObject)value)
            {
                JsonNode propVal = property.Value;

                emitter.Emit(new Scalar(null, property.Key));

                if (property.Value == null)
                {
                    WriteValue(emitter, null);
                }
                else
                {
                    WriteYaml(emitter, propVal, propVal.GetType());
                }
            }

            emitter.Emit(new MappingEnd());
        }

        private void WriteValue(IEmitter emitter, object value)
        {
            if (value == null)
            {
                emitter.Emit(new Scalar(null, "null"));
                return;
            }

            var obj = ((JsonValue)value).GetValue<JsonElement>();

            switch (obj.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    throw new NotImplementedException();
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
                            float.TryParse(val, out var _) ||
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
                default:
                    emitter.Emit(new Scalar(obj.ToString()));
                    break;
            }
        }

        private void WriteArray(IEmitter emitter, object value)
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

                WriteYaml(emitter, item, item.GetType());
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}