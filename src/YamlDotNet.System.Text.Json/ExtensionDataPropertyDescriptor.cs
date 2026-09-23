using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

internal sealed class ExtensionDataPropertyDescriptor : IPropertyDescriptor
{
    private readonly IPropertyDescriptor _baseDescriptor;

    public ExtensionDataPropertyDescriptor(IPropertyDescriptor baseDescriptor)
    {
        _baseDescriptor = baseDescriptor;
        Name = baseDescriptor.Name;
    }

    public bool AllowNulls { get; set; }

    public string Name { get; set; }

    public bool Required => false;

    public Type Type => typeof(object);

    public Type? TypeOverride { get; set; }

    public Type? ConverterType { get; set; }

    public int Order { get; set; }

    public ScalarStyle ScalarStyle { get; set; }

    public bool CanWrite { get; set; }

    public void Write(object target, object? value)
    {
        var (dict, type) = GetOrCreateExtensionDataDictionary(target, _baseDescriptor);

        if (type == typeof(JsonElement))
        {
            dict[Name] = ToJsonElement(value);
        }
        else
        {
            dict[Name] = value;
        }
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        return _baseDescriptor.GetCustomAttribute<T>();
    }

    private static JsonElement ToJsonElement(object? value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            WriteJsonValue(writer, value);
        }

        using var json = JsonDocument.Parse(stream.ToArray());
        return json.RootElement.Clone();
    }

    private static void WriteJsonValue(Utf8JsonWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case JsonElement element:
                element.WriteTo(writer);
                break;
            case JsonDocument document:
                document.RootElement.WriteTo(writer);
                break;
            case JsonNode node:
                node.WriteTo(writer);
                break;
            case string text:
                writer.WriteStringValue(text);
                break;
            case char character:
                writer.WriteStringValue(character.ToString());
                break;
            case bool boolean:
                writer.WriteBooleanValue(boolean);
                break;
            case byte[] bytes:
                writer.WriteBase64StringValue(bytes);
                break;
            case byte number:
                writer.WriteNumberValue(number);
                break;
            case sbyte number:
                writer.WriteNumberValue(number);
                break;
            case short number:
                writer.WriteNumberValue(number);
                break;
            case ushort number:
                writer.WriteNumberValue(number);
                break;
            case int number:
                writer.WriteNumberValue(number);
                break;
            case uint number:
                writer.WriteNumberValue(number);
                break;
            case long number:
                writer.WriteNumberValue(number);
                break;
            case ulong number:
                writer.WriteNumberValue(number);
                break;
            case float number:
                writer.WriteNumberValue(number);
                break;
            case double number:
                writer.WriteNumberValue(number);
                break;
            case decimal number:
                writer.WriteNumberValue(number);
                break;
            case DateTime dateTime:
                writer.WriteStringValue(dateTime);
                break;
            case DateTimeOffset dateTimeOffset:
                writer.WriteStringValue(dateTimeOffset);
                break;
            case Guid guid:
                writer.WriteStringValue(guid);
                break;
            case Enum enumValue:
                WriteEnumValue(writer, enumValue);
                break;
            case IDictionary dictionary:
                writer.WriteStartObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key is not string key)
                    {
                        throw new InvalidOperationException("Extension data JSON objects must have string keys.");
                    }

                    writer.WritePropertyName(key);
                    WriteJsonValue(writer, entry.Value);
                }
                writer.WriteEndObject();
                break;
            case IEnumerable sequence:
                writer.WriteStartArray();
                foreach (var item in sequence)
                {
                    WriteJsonValue(writer, item);
                }
                writer.WriteEndArray();
                break;
            default:
                throw new InvalidOperationException($"Extension data value type '{value.GetType().FullName}' cannot be converted without runtime JSON metadata. Use JSON values, supported scalars, dictionaries, or sequences.");
        }
    }

    private static void WriteEnumValue(Utf8JsonWriter writer, Enum value)
    {
        var underlyingType = Enum.GetUnderlyingType(value.GetType());
        if (underlyingType == typeof(ulong))
        {
            writer.WriteNumberValue(Convert.ToUInt64(value, CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNumberValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
        }
    }

    public IObjectDescriptor Read(object target)
    {
        var (dict, _) = GetOrCreateExtensionDataDictionary(target, _baseDescriptor);

        var item = dict[Name];

        return new ObjectDescriptor(item, item?.GetType() ?? typeof(object), item?.GetType() ?? typeof(object));
    }

    private static (Type key, Type val)? GetIDictionaryKVTypes([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type t)
    {
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>))
        {
            var arguments = t.GetGenericArguments();
            return (arguments[0], arguments[1]);
        }

        foreach (var interfaceType in t.GetInterfaces())
        {
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var arguments = interfaceType.GetGenericArguments();
                return (arguments[0], arguments[1]);
            }
        }

        return null;
    }

#if !NETSTANDARD2_0
    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "IPropertyDescriptor.Type cannot express interface preservation. StaticContext registrations root the concrete extension-data dictionary type.")]
#endif
    private static (IDictionary, Type) GetOrCreateExtensionDataDictionary(object target, IPropertyDescriptor prop)
    {
        var (_, val) = GetIDictionaryKVTypes(prop.Type)
                 ?? throw new InvalidOperationException("ExtensionData must be an IDictionary<TKey, TValue>.");

        if (val == typeof(object))
        {
            if (prop.Read(target).Value is not IDictionary<string, object> dict)
            {
                dict = CreateDictObject(prop.Type);
                prop.Write(target, dict);
            }
            return ((IDictionary)dict, val);
        }

        if (val == typeof(JsonElement))
        {
            if (prop.Read(target).Value is not IDictionary<string, JsonElement> dict)
            {
                dict = CreateDictJsonElement(prop.Type);
                prop.Write(target, dict);
            }
            return ((IDictionary)dict, val);
        }

        throw new InvalidOperationException($"Extension data property must be of type IDictionary<string, object> or IDictionary<string, JsonElement>. Found: {val.FullName}.");
    }

    private static IDictionary<string, object> CreateDictObject(Type propertyType)
    {
        if (propertyType.IsAssignableFrom(typeof(Dictionary<string, object>)))
        {
            return new Dictionary<string, object>();
        }

        throw new InvalidOperationException($"Extension data dictionary type '{propertyType.FullName}' cannot be created without reflection. Initialize the property before deserialization or use a type assignable from Dictionary<string, object>.");
    }

    private static IDictionary<string, JsonElement> CreateDictJsonElement(Type propertyType)
    {
        if (propertyType.IsAssignableFrom(typeof(Dictionary<string, JsonElement>)))
        {
            return new Dictionary<string, JsonElement>();
        }

        throw new InvalidOperationException($"Extension data dictionary type '{propertyType.FullName}' cannot be created without reflection. Initialize the property before deserialization or use a type assignable from Dictionary<string, JsonElement>.");
    }
}
