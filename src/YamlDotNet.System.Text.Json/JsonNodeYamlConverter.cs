using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
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
            return type == typeof(JsonNode) || type == typeof(JsonArray) || type == typeof(JsonObject) || type == typeof(JsonValue) || type.BaseType == typeof(JsonValue);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (type == typeof(JsonValue) || type.BaseType == typeof(JsonValue))
            {
                WriteValue(emitter, value);
            }
            else if (type == typeof(JsonObject) || type.BaseType == typeof(JsonObject))
            {
                WriteObject(emitter, value);
            }
            else if (type == typeof(JsonArray) || type.BaseType == typeof(JsonArray))
            {
                WriteArray(emitter, value);
            }
        }

        private void WriteObject(IEmitter emitter, object value)
        {
            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Any));

            JsonObject obj = (JsonObject)value;

            foreach (var property in obj)
            {
                JsonNode propVal = property.Value;
                emitter.Emit(new Scalar(null, property.Key));
                WriteYaml(emitter, propVal, propVal.GetType());
            }

            emitter.Emit(new MappingEnd());
        }

        private void WriteValue(IEmitter emitter, object value)
        {

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