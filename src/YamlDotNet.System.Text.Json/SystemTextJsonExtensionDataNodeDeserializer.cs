using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Helpers;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Provides YAML deserialization support for types with extension data properties, enabling additional unmapped data to
/// be captured during deserialization.
/// </summary>
public sealed class SystemTextJsonExtensionDataNodeDeserializer : INodeDeserializer
{
    private readonly INodeDeserializer _inner;
    private readonly ITypeInspector _typeInspector;

    /// <summary>
    /// Initializes a new instance of the SystemTextJsonExtensionDataNodeDeserializer class using the specified inner
    /// node deserializer.
    /// </summary>
    /// <param name="inner">The inner node deserializer to use for delegating deserialization operations. Cannot be null.</param>
    /// <param name="typeInspector">The ITypeInspector. Cannot be null.</param>
    public SystemTextJsonExtensionDataNodeDeserializer(INodeDeserializer inner, ITypeInspector typeInspector)
    {
        _inner = inner;
        _typeInspector = typeInspector;
    }

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
        var extenstionDataProperty = _typeInspector.GetProperties(expectedType, null).FirstOrDefault(x => x.GetCustomAttribute<JsonExtensionDataAttribute>() != null);

        if (extenstionDataProperty is null)
        {
            return _inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }

        if (!reader.TryConsume<MappingStart>(out var mapStart))
        {
            return _inner.Deserialize(reader, expectedType, nestedObjectDeserializer, out value, rootDeserializer);
        }

        var implementationType = Nullable.GetUnderlyingType(expectedType)
            ?? FsharpHelper.GetOptionUnderlyingType(expectedType)
            ?? expectedType;

        var instance = Activator.CreateInstance(implementationType)
            ?? throw new InvalidOperationException($"Cannot create instance of {implementationType}. Add a parameterless constructor.");

        var (extensionDict, extenstionType) = GetOrCreateExtensionDataDictionary(instance, extenstionDataProperty);

        while (!reader.Accept<MappingEnd>(out _))
        {
            if (!reader.TryConsume<Scalar>(out var propertyName))
            {
                throw new YamlException(mapStart.Start, mapStart.End, "Only scalar mapping keys are supported.");
            }

            var property = _typeInspector.GetProperty(implementationType, null, propertyName.Value, true, false);

            if (property != null)
            {
                var v = nestedObjectDeserializer(reader, property.Type);
                property.Write(instance, v);
            }
            else
            {
                var v = nestedObjectDeserializer(reader, typeof(object));

                if (extenstionType == typeof(JsonElement))
                {
                    v = JsonSerializer.SerializeToElement(v);
                }
                extensionDict.Add(propertyName.Value, v);
            }
        }

        reader.Consume<MappingEnd>();
        value = instance;
        return true;
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

    /// <summary>
    /// Gets the extension data dictionary associated with the specified target object and property descriptor, or
    /// creates and assigns a compatible dictionary if one does not exist.
    /// </summary>
    /// <remarks>Only dictionaries of type <see cref="Dictionary{TKey,TValue}"/> (with <c>TKey</c> as <see cref="string"/> and <c>TValue</c> as <see cref="object"/> or <see cref="JsonElement"/>) are supported for extension data. If the property does not contain a
    /// compatible dictionary, a new instance will be created and assigned.</remarks>
    /// <param name="target">The target object that contains the extension data property.</param>
    /// <param name="prop">The property descriptor representing the extension data property. The property type must be a supported
    /// dictionary type.</param>
    /// <returns>A tuple containing the extension data dictionary as an <see cref="IDictionary"/> and the value type of the
    /// dictionary. The dictionary will be created and assigned to the property if it does not already exist.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property type is not a supported dictionary type, or if the dictionary value type is not <see
    /// cref="object"/> or <see cref="JsonElement"/>.</exception>
    public static (IDictionary, Type) GetOrCreateExtensionDataDictionary(object target, IPropertyDescriptor prop)
    {
        var (_, val) = GetIdictionaryKVTypes(prop.Type)
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

        throw new InvalidOperationException("Only Dictionary<string, object> or Dictionary<string, JsonElement> are supported for ExtensionData.");
    }

    private static IDictionary<string, object> CreateDictObject(Type propertyType)
    {
        if (!propertyType.IsInterface && !propertyType.IsAbstract)
        {
            return (IDictionary<string, object>)(Activator.CreateInstance(propertyType)
                    ?? new Dictionary<string, object>());
        }

        return new Dictionary<string, object>();
    }

    private static IDictionary<string, JsonElement> CreateDictJsonElement(Type propertyType)
    {
        if (!propertyType.IsInterface && !propertyType.IsAbstract)
        {
            return (IDictionary<string, JsonElement>)(Activator.CreateInstance(propertyType)
                    ?? new Dictionary<string, JsonElement>());
        }

        return new Dictionary<string, JsonElement>();
    }
}
