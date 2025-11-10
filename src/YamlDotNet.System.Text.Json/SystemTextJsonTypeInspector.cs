using System.Reflection;
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

    /// <summary>
    /// Applies property settings from <see cref="JsonPropertyNameAttribute"/> and <see cref="JsonIgnoreAttribute"/> and <see cref="JsonStringEnumMemberNameAttribute"/> to YamlDotNet
    /// </summary>
    /// <param name="innerTypeDescriptor"></param>
    /// <param name="ignoreOrder">ignores JsonPropertyOrder</param>
    public SystemTextJsonTypeInspector(ITypeInspector innerTypeDescriptor, bool ignoreOrder = false)
    {
        _innerTypeDescriptor = innerTypeDescriptor;
        _ignoreOrder = ignoreOrder;
    }

    /// <summary>
    /// Returns the name of the enum member in the specified type that is associated with the given JSON string value,
    /// if such a mapping exists.
    /// </summary>
    /// <remarks>If no enum member in <paramref name="enumType"/> is decorated with a matching
    /// <see cref="JsonStringEnumMemberNameAttribute" />, the method delegates to the underlying type descriptor to resolve the
    /// name. This method is useful when working with enums that use custom string representations for JSON
    /// serialization.</remarks>
    /// <param name="enumType">The type of the enumeration to search for a member name. Must be a valid enum type.</param>
    /// <param name="name">The JSON string value to match against the enum member's custom name attribute.</param>
    /// <returns>The name of the enum member that corresponds to the specified JSON string value, or the result from the
    /// underlying type descriptor if no match is found.</returns>
    public string GetEnumName(Type enumType, string name)
    {
        foreach (var mi in enumType.GetMembers(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = mi.GetCustomAttribute<JsonStringEnumMemberNameAttribute>();
            if (attr != null && attr.Name.Equals(name, StringComparison.Ordinal))
            {
                return mi.Name;
            }
        }

        return _innerTypeDescriptor.GetEnumName(enumType, name);
    }

    /// <summary>
    /// Returns the string representation of the specified enum value, using a custom name if defined by a
    /// <see cref="JsonStringEnumMemberNameAttribute" />.
    /// </summary>
    /// <remarks>If the enum member is decorated with a <see cref="JsonStringEnumMemberNameAttribute" />, its Name property
    /// is used as the string representation. If no such attribute is found, the method falls back to the default
    /// behavior provided by innerTypeDescriptor. This method does not perform validation on the type of enumValue;
    /// callers should ensure that enumValue is a valid enum member.</remarks>
    /// <param name="enumValue">The enum value for which to retrieve the string representation. Must be a valid member of an enumeration type.</param>
    /// <returns>A string containing the custom name defined by <see cref="JsonStringEnumMemberNameAttribute" /> if present; otherwise, the
    /// default string representation of the enum value.</returns>
    public string GetEnumValue(object enumValue)
    {
        var type = enumValue.GetType();

        foreach (var mi in type.GetMembers(BindingFlags.Public | BindingFlags.Static))
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

    /// <summary>
    /// Returns a collection of property descriptors for the specified type, including declared properties and any
    /// extension data properties present in the container.
    /// </summary>
    /// <remarks>Properties marked with <see cref="JsonIgnoreAttribute"/> and a condition of <see
    /// cref="JsonIgnoreCondition.Always"/> are excluded from the result. Properties with <see
    /// cref="JsonExtensionDataAttribute"/> are expanded to include each extension data entry as a separate property
    /// descriptor. The order of returned properties may be affected by <see cref="JsonPropertyOrderAttribute"/> unless
    /// ordering is ignored.</remarks>
    /// <param name="type">The type whose properties are to be described. Must not be null.</param>
    /// <param name="container">An optional object instance that may contain extension data properties. If null, only declared properties are
    /// processed.</param>
    /// <returns>An enumerable collection of <see cref="IPropertyDescriptor"/> objects representing the properties of the
    /// specified type. The collection may include extension data properties if present in the container.</returns>
    public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        // First, process declared properties as before.
        var declaredProperties = _innerTypeDescriptor.GetProperties(type, container)
            .Where(p =>
            {
                var ignore = p.GetCustomAttribute<JsonIgnoreAttribute>();
                return ignore == null ||
                       ignore.Condition == JsonIgnoreCondition.Never ||
                       ignore.Condition != JsonIgnoreCondition.Always;
            })
            .SelectMany(p =>
            {
                if (p.GetCustomAttribute<JsonExtensionDataAttribute>() != null)
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

                    var nameAttribute = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                    if (nameAttribute != null)
                    {
                        descriptor.Name = nameAttribute.Name;
                    }

                    if (!_ignoreOrder)
                    {
                        var orderAttribute = p.GetCustomAttribute<JsonPropertyOrderAttribute>();
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

    /// <summary>
    /// Retrieves the property descriptor for the specified property name from the given type and container, with
    /// options for case-insensitive matching and handling unmatched properties.
    /// </summary>
    /// <remarks>If the type contains a property marked with <see cref="JsonExtensionDataAttribute"/> and no
    /// direct match is found, that property will be returned to support arbitrary property assignment.</remarks>
    /// <param name="type">The type that contains the property to retrieve.</param>
    /// <param name="container">An optional instance of the object that may influence property resolution. Can be null if not required.</param>
    /// <param name="name">The name of the property to locate. Matching can be case-sensitive or case-insensitive based on the specified
    /// option.</param>
    /// <param name="ignoreUnmatched">Not used</param>
    /// <param name="caseInsensitivePropertyMatching">If set to <see langword="true"/>, property name matching is performed without regard to case; otherwise,
    /// matching is case-sensitive.</param>
    /// <returns>An <see cref="IPropertyDescriptor"/> representing the matched property, or null if no match is found and
    /// <paramref name="ignoreUnmatched"/> is <see langword="true"/>.</returns>
    /// <exception cref="SerializationException">Thrown if no matching property is found and <paramref name="ignoreUnmatched"/> is <see langword="false"/>, or if
    /// multiple properties match the specified name.</exception>
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
            var jsonExtensionData = GetProperties(type, container).First(x => x.GetCustomAttribute<JsonExtensionDataAttribute>() != null);

            var prop = new ExtensionDataPropertyDescriptor(jsonExtensionData)
            {
                Name = name,
            };

            return prop;
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
}
