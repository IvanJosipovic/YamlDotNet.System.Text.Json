using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Provides YAML deserialization support for types with extension data properties, enabling additional unmapped data to
/// be captured during deserialization.
/// </summary>
/// <remarks>This class decorates an existing <see cref="INodeDeserializer"/> and adds handling for properties
/// marked with <see cref="System.Text.Json.Serialization.JsonExtensionDataAttribute"/>. When deserializing a mapping
/// node, any keys not matching known properties are stored in the extension data dictionary, allowing round-tripping of
/// extra YAML fields. Only extension data properties of type <see cref="IDictionary{string, object}"/> or <see
/// cref="IDictionary{string, JsonElement}"/> are supported. This class is sealed and intended for use within custom
/// YAML deserialization pipelines.</remarks>
public sealed class SystemTextJsonExtensionDataNodeDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _inner;

    public SystemTextJsonExtensionDataNodeDeserializer(INodeDeserializer inner) => _inner = inner;

    public bool Deserialize(
        IParser reader,
        Type expectedType,
        Func<IParser, Type, object?> nestedObjectDeserializer,
        out object? value,
        ObjectDeserializer rootDeserializer)
    {
        var extProp = FindExtensionDictProperty(expectedType);
        if (extProp is null)
            return _inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);

        if (!reader.TryConsume<MappingStart>(out var mapStart))
            return _inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);

        var instance = Activator.CreateInstance(expectedType)
            ?? throw new InvalidOperationException($"Cannot create instance of {expectedType}. Add a parameterless constructor.");

        var known = GetWritableProps(expectedType);
        var bag = GetOrCreateExtBag(instance, extProp);

        while (!reader.Accept<MappingEnd>(out _))
        {
            if (!reader.TryConsume<Scalar>(out var keyScalar))
                throw new YamlException(mapStart.Start, reader.Current.Start, "Only scalar mapping keys are supported.");

            var key = keyScalar.Value ?? string.Empty;

            if (known.TryGetValue(key, out var pd))
            {
                var v = nestedObjectDeserializer(reader, pd.PropertyType);
                pd.SetValue(instance, v);
            }
            else
            {
                var v = nestedObjectDeserializer(reader, typeof(object));
                bag.Put(key, v);
            }
        }

        reader.Consume<MappingEnd>();
        value = instance;
        return true;
    }

    private static PropertyInfo? FindExtensionDictProperty(Type t)
    {
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (p.GetCustomAttribute<JsonExtensionDataAttribute>() is null) continue;

            var kv = GetIdictionaryKVTypes(p.PropertyType);
            if (kv is null) continue;

            var (keyT, valT) = kv.Value;
            if (keyT == typeof(string) && (valT == typeof(object) || valT == typeof(JsonElement)))
                return p;
        }
        return null;
    }

    private static (Type key, Type val)? GetIdictionaryKVTypes(Type t)
    {
        // check the type itself
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDictionary<,>))
        {
            var a = t.GetGenericArguments();
            return (a[0], a[1]);
        }

        // check implemented interfaces
        foreach (var i in t.GetInterfaces())
        {
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var a = i.GetGenericArguments();
                return (a[0], a[1]);
            }
        }
        return null;
    }

    private sealed class ExtensionBag
    {
        public Action<string, object?> Put { get; }
        public ExtensionBag(Action<string, object?> put) { Put = put; }
    }

    private static ExtensionBag GetOrCreateExtBag(object target, PropertyInfo prop)
    {
        var (key, val) = GetIdictionaryKVTypes(prop.PropertyType)
                 ?? throw new InvalidOperationException("ExtensionData must be an IDictionary<TKey, TValue>.");

        var valT = val;

        if (valT == typeof(object))
        {
            if (prop.GetValue(target) is not IDictionary<string, object> dict)
            {
                dict = CreateDictObject(prop.PropertyType);
                prop.SetValue(target, dict);
            }
            return new ExtensionBag((k, v) => dict[k] = v!);
        }

        if (valT == typeof(JsonElement))
        {
            if (prop.GetValue(target) is not IDictionary<string, JsonElement> dict)
            {
                dict = CreateDictJsonElement(prop.PropertyType);
                prop.SetValue(target, dict);
            }
            return new ExtensionBag((k, v) => dict[k] = ToJsonElement(v));
        }

        throw new InvalidOperationException("Only Dictionary<string, object> or Dictionary<string, JsonElement> are supported for ExtensionData.");
    }

    private static IDictionary<string, object> CreateDictObject(Type propertyType)
    {
        if (!propertyType.IsInterface && !propertyType.IsAbstract)
            return (IDictionary<string, object>)(Activator.CreateInstance(propertyType)
                    ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));

        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    private static IDictionary<string, JsonElement> CreateDictJsonElement(Type propertyType)
    {
        if (!propertyType.IsInterface && !propertyType.IsAbstract)
            return (IDictionary<string, JsonElement>)(Activator.CreateInstance(propertyType)
                    ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase));

        return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
    }

    private static JsonElement ToJsonElement(object? value)
    {
        if (value is JsonElement je) return je.Clone();

        // serialize just this subtree to produce a JsonElement
        // handles nulls, scalars, sequences, and mappings
        return JsonSerializer.SerializeToElement(value, value?.GetType() ?? typeof(object));
    }

    private static Dictionary<string, PropertyInfo> GetWritableProps(Type t)
    {
        var dict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!p.CanWrite) continue;
            if (p.GetCustomAttribute<JsonExtensionDataAttribute>() != null) continue;

            var alias = p.GetCustomAttribute<YamlMemberAttribute>()?.Alias;
            if (!string.IsNullOrWhiteSpace(alias)) dict[alias!] = p;

            dict[p.Name] = p;
        }
        return dict;
    }
}