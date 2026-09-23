using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Applies property settings from <see cref="JsonPropertyNameAttribute"/> and <see cref="JsonIgnoreAttribute"/> and <see cref="JsonStringEnumMemberNameAttribute"/> to YamlDotNet
/// </summary>
public sealed class SystemTextJsonTypeInspector : ITypeInspector
{
    private readonly ITypeInspector _innerTypeDescriptor;

    private readonly bool _ignoreOrder;

    /// <inheritdoc />
    public SystemTextJsonTypeInspector(ITypeInspector innerTypeDescriptor, bool ignoreOrder = false)
    {
        _innerTypeDescriptor = innerTypeDescriptor;
        _ignoreOrder = ignoreOrder;
    }

    /// <inheritdoc />
#if !NETSTANDARD2_0
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "ITypeInspector does not propagate enum Type member requirements. Trimmed callers must preserve public enum fields for JsonStringEnumMemberNameAttribute.")]
#endif
    public string GetEnumName(Type enumType, string name)
    {
        foreach (var mi in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = mi.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attr != null && attr.Name.Equals(name, StringComparison.Ordinal))
            {
                return mi.Name;
            }
        }

        return _innerTypeDescriptor.GetEnumName(enumType, name);
    }

    /// <inheritdoc />
#if !NETSTANDARD2_0
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "ITypeInspector receives runtime enum values without member requirements. Trimmed callers must preserve public enum fields for JsonStringEnumMemberNameAttribute.")]
#endif
    public string GetEnumValue(object enumValue)
    {
        var type = enumValue.GetType();

        foreach (var mi in type.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var value = Enum.Parse(type, mi.Name);

            if (enumValue.Equals(value))
            {
                var attr = mi.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
                if (attr != null)
                {
                    return attr.Name;
                }

                break;
            }
        }

        return _innerTypeDescriptor.GetEnumValue(enumValue);
    }

    /// <inheritdoc />
    public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        // First, process declared properties as before.
        var declaredProperties = _innerTypeDescriptor.GetProperties(type, container)
            .Where(p => ShouldIncludeProperty(type, p, container))
            .SelectMany(p =>
            {
                if (GetCustomAttribute<JsonExtensionDataAttribute>(type, p) != null)
                {
                    if (container == null)
                    {
                        return [p];
                    }

                    var props = new List<IPropertyDescriptor>();

                    if (p.Read(container).Value is IDictionary<string, JsonElement> extData)
                    {
                        foreach (var entry in extData)
                        {
                            // Create a property descriptor for each extension data key/value.
                            var extProp = new ExtensionDataPropertyDescriptor(p)
                            {
                                Name = entry.Key,
                            };
                            props.Add(extProp);
                        }
                    }
                    else if (p.Read(container).Value is IDictionary<string, object> extData2)
                    {
                        foreach (var entry in extData2)
                        {
                            // Create a property descriptor for each extension data key/value.
                            var extProp = new ExtensionDataPropertyDescriptor(p)
                            {
                                Name = entry.Key,
                            }; props.Add(extProp);
                        }
                    }

                    return props;
                }
                else
                {
                    var descriptor = new PropertyDescriptor(p);

                    var nameAttribute = GetCustomAttribute<JsonPropertyNameAttribute>(type, p);
                    if (nameAttribute != null)
                    {
                        descriptor.Name = nameAttribute.Name;
                    }

                    if (!_ignoreOrder)
                    {
                        var orderAttribute = GetCustomAttribute<JsonPropertyOrderAttribute>(type, p);
                        if (orderAttribute != null)
                        {
                            descriptor.Order = orderAttribute.Order;
                        }
                    }

                    return [descriptor];
                }
            });

        // Combine declared and extension properties.
        if (_ignoreOrder)
        {
            return declaredProperties;
        }

        return declaredProperties.OrderBy(p => p.Order);
    }

    private static bool ShouldIncludeProperty(Type type, IPropertyDescriptor property, object? container)
    {
        var ignore = GetCustomAttribute<JsonIgnoreAttribute>(type, property);
        if (ignore == null)
        {
            return true;
        }

        return ignore.Condition switch
        {
            JsonIgnoreCondition.Always => false,
            JsonIgnoreCondition.Never => true,
            JsonIgnoreCondition.WhenWritingNull => container == null || property.Read(container).Value is not null,
            JsonIgnoreCondition.WhenWritingDefault => container == null || !IsDefaultValue(property, container),
            JsonIgnoreCondition.WhenWriting => container == null,
            JsonIgnoreCondition.WhenReading => container != null,
            _ => throw new ArgumentOutOfRangeException(nameof(property), ignore.Condition, "Unsupported JsonIgnoreCondition value."),
        };
    }

    private static bool IsDefaultValue(IPropertyDescriptor property, object container)
    {
        var value = property.Read(container).Value;
        return value == null || property.Type.IsValueType && value.Equals(Array.CreateInstance(property.Type, 1).GetValue(0));
    }

    /// <inheritdoc />
    public IPropertyDescriptor GetProperty(Type type, object? container, string name, bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
    {
        IEnumerable<IPropertyDescriptor> candidates;

        if (caseInsensitivePropertyMatching)
        {
            candidates = GetProperties(type, container)
                .Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            candidates = GetProperties(type, container)
                .Where(p => p.Name == name);
        }

        using var enumerator = candidates.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            var ignoredProperty = _innerTypeDescriptor.GetProperties(type, container)
                .Where(p => GetCustomAttribute<JsonIgnoreAttribute>(type, p)?.Condition is JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenReading)
                .Select(p => GetCustomAttribute<JsonPropertyNameAttribute>(type, p)?.Name ?? p.Name)
                .FirstOrDefault(propertyName => caseInsensitivePropertyMatching
                    ? propertyName.Equals(name, StringComparison.OrdinalIgnoreCase)
                    : propertyName == name);

            if (ignoredProperty != null)
            {
                return null!;
            }

            var jsonExtensionData = _innerTypeDescriptor.GetProperties(type, container)
                .FirstOrDefault(x => GetCustomAttribute<JsonExtensionDataAttribute>(type, x) != null);

            if (jsonExtensionData != null)
            {
                var prop = new ExtensionDataPropertyDescriptor(jsonExtensionData)
                {
                    Name = name,
                };

                return prop;
            }

            if (ignoreUnmatched)
            {
                return null!;
            }

            throw new SerializationException($"Property '{name}' not found on type '{type.FullName}'.");
        }

        var property = enumerator.Current;

        if (enumerator.MoveNext())
        {
            throw new SerializationException(
                $"Multiple properties with the name/alias '{name}' already exists on type '{type.FullName}', maybe you're misusing JsonPropertyName or maybe you are using the wrong naming convention? The matching properties are: {string.Join(", ", [.. candidates.Select(p => p.Name)])}"
            );
        }

        return property;
    }

    /// <inheritdoc />
    public bool HasParseMethod(Type type)
    {
        return false;
    }

    /// <inheritdoc />
    public object? Parse(string value, Type expectedType)
    {
        throw new NotImplementedException();
    }

#if !NETSTANDARD2_0
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "StaticContext registrations root public DTO properties, but ITypeInspector cannot express that requirement on its Type parameter.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "StaticContext registrations root public DTO properties, but ITypeInspector cannot express that requirement on its Type parameter.")]
#endif
    private static TAttribute? GetCustomAttribute<TAttribute>(Type type, IPropertyDescriptor property)
        where TAttribute : Attribute
    {
        return property.GetCustomAttribute<TAttribute>()
            ?? type.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public)?.GetCustomAttribute<TAttribute>();
    }
}
