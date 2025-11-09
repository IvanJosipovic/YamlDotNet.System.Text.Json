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
public sealed class SystemTextJsonExtensionDataNodeDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _inner;

    /// <summary>
    /// Initializes a new instance of the SystemTextJsonExtensionDataNodeDeserializer class using the specified inner
    /// node deserializer.
    /// </summary>
    /// <param name="inner">The inner node deserializer to use for delegating deserialization operations. Cannot be null.</param>
    public SystemTextJsonExtensionDataNodeDeserializer(INodeDeserializer inner) => _inner = inner;

    /// <summary>
    /// Deserializes a YAML mapping into an object of the specified type, populating known properties and storing
    /// unknown keys in an extension dictionary if present.
    /// </summary>
    /// <remarks>If the target type defines an extension dictionary property, unknown YAML keys are stored in
    /// that dictionary. Known properties are set using the provided deserializer delegate. The method returns false if
    /// the input does not represent a mapping or if deserialization is not possible.</remarks>
    /// <param name="reader">The parser used to read YAML events from the input stream.</param>
    /// <param name="expectedType">The type of object to create and populate from the YAML mapping.</param>
    /// <param name="nestedObjectDeserializer">A delegate used to deserialize nested objects or property values from the parser.</param>
    /// <param name="value">When this method returns, contains the deserialized object if successful; otherwise, <see langword="null"/>.</param>
    /// <param name="rootDeserializer">The root object deserializer used for handling complex or nested deserialization scenarios.</param>
    /// <returns>true if the YAML mapping was successfully deserialized into an object; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if an instance of <paramref name="expectedType"/> cannot be created, typically due to a missing
    /// parameterless constructor.</exception>
    /// <exception cref="YamlException">Thrown if a mapping key in the YAML is not a scalar value.</exception>
    public bool Deserialize(
        IParser reader,
        Type expectedType,
        Func<IParser, Type, object?> nestedObjectDeserializer,
        out object? value,
        ObjectDeserializer rootDeserializer)
    {
        var extProp = FindExtensionDictProperty(expectedType);
        if (extProp is null)
        {
            return _inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }

        if (!reader.TryConsume<MappingStart>(out var mapStart))
        {
            return _inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }

        var instance = Activator.CreateInstance(expectedType)
            ?? throw new InvalidOperationException($"Cannot create instance of {expectedType}. Add a parameterless constructor.");

        var known = GetWritableProps(expectedType);
        var bag = GetOrCreateExtBag(instance, extProp);

        while (!reader.Accept<MappingEnd>(out _))
        {
            if (!reader.TryConsume<Scalar>(out var keyScalar))
            {
                throw new YamlException(mapStart.Start, mapStart.End, "Only scalar mapping keys are supported.");
            }

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

    private static PropertyInfo? FindExtensionDictProperty(Type type)
    {
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetCustomAttribute<JsonExtensionDataAttribute>() is null)
            {
                continue;
            }

            var kv = GetIdictionaryKVTypes(property.PropertyType);
            if (kv is null)
            {
                continue;
            }

            var (keyT, valT) = kv.Value;
            if (keyT == typeof(string) && (valT == typeof(object) || valT == typeof(JsonElement)))
            {
                return property;
            }
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
        public ExtensionBag(Action<string, object?> put)
        {
            Put = put;
        }
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
        {
            return (IDictionary<string, object>)(Activator.CreateInstance(propertyType)
                    ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase));
        }

        return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }

    private static IDictionary<string, JsonElement> CreateDictJsonElement(Type propertyType)
    {
        if (!propertyType.IsInterface && !propertyType.IsAbstract)
        {
            return (IDictionary<string, JsonElement>)(Activator.CreateInstance(propertyType)
                    ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase));
        }

        return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
    }

    private static JsonElement ToJsonElement(object? value)
    {
        if (value is JsonElement je)
        {
            return je.Clone();
        }

        // serialize just this subtree to produce a JsonElement
        // handles nulls, scalars, sequences, and mappings
        return JsonSerializer.SerializeToElement(value, value?.GetType() ?? typeof(object));
    }

    private static Dictionary<string, PropertyInfo> GetWritableProps(Type type)
    {
        var dict = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanWrite)
            {
                continue;
            }

            if (property.GetCustomAttribute<JsonExtensionDataAttribute>() != null)
            {
                continue;
            }

            var alias = property.GetCustomAttribute<YamlMemberAttribute>()?.Alias;
            if (!string.IsNullOrWhiteSpace(alias))
            {
                dict[alias!] = property;
            }

            dict[property.Name] = property;
        }
        return dict;
    }
}
