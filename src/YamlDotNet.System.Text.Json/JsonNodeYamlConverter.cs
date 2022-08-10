using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
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
            throw new NotImplementedException();
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

                WriteYaml(emitter, propVal, propVal.GetType());
            }

            emitter.Emit(new MappingEnd());
        }

        private void WriteValue(IEmitter emitter, object value)
        {
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
                WriteYaml(emitter, item, item.GetType());
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}