using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Core;
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
        return GetPropertyDescriptors(GetInspectedProperties(type, container), container);
    }

    private IEnumerable<IPropertyDescriptor> GetPropertyDescriptors(IReadOnlyList<InspectedProperty> properties, object? container)
    {
        var declaredProperties = properties
            .Where(p => ShouldIncludeProperty(p, container))
            .SelectMany(inspectedProperty =>
            {
                var property = inspectedProperty.Descriptor;
                if (GetCustomAttribute<JsonExtensionDataAttribute>(inspectedProperty) != null)
                {
                    if (container == null)
                    {
                        return [property];
                    }

                    var props = new List<IPropertyDescriptor>();

                    if (property.Read(container).Value is IDictionary<string, JsonElement> extData)
                    {
                        foreach (var entry in extData)
                        {
                            // Create a property descriptor for each extension data key/value.
                            var extProp = new ExtensionDataPropertyDescriptor(property)
                            {
                                Name = entry.Key,
                            };
                            props.Add(extProp);
                        }
                    }
                    else if (property.Read(container).Value is IDictionary<string, object> extData2)
                    {
                        foreach (var entry in extData2)
                        {
                            // Create a property descriptor for each extension data key/value.
                            var extProp = new ExtensionDataPropertyDescriptor(property)
                            {
                                Name = entry.Key,
                            }; props.Add(extProp);
                        }
                    }

                    return props;
                }
                else
                {
                    var descriptor = new PropertyDescriptor(property);

                    var nameAttribute = GetCustomAttribute<JsonPropertyNameAttribute>(inspectedProperty);
                    if (nameAttribute != null)
                    {
                        descriptor.Name = nameAttribute.Name;
                    }

                    if (!_ignoreOrder)
                    {
                        var orderAttribute = GetCustomAttribute<JsonPropertyOrderAttribute>(inspectedProperty);
                        if (orderAttribute != null)
                        {
                            descriptor.Order = orderAttribute.Order;
                        }
                    }

                    if (property.Type.IsEnum && GetCustomAttribute<global::System.ComponentModel.DefaultValueAttribute>(inspectedProperty) == null)
                    {
                        var enumDefault = Array.CreateInstance(property.Type, 1).GetValue(0)!;
                        return [new EnumDefaultValuePropertyDescriptor(descriptor, new global::System.ComponentModel.DefaultValueAttribute(enumDefault))];
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

    private static bool ShouldIncludeProperty(InspectedProperty inspectedProperty, object? container)
    {
        var property = inspectedProperty.Descriptor;
        var ignore = GetCustomAttribute<JsonIgnoreAttribute>(inspectedProperty);
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
            _ => throw new ArgumentOutOfRangeException(nameof(inspectedProperty), ignore.Condition, "Unsupported JsonIgnoreCondition value."),
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
        var inspectedProperties = GetInspectedProperties(type, container);
        IEnumerable<IPropertyDescriptor> candidates;

        if (caseInsensitivePropertyMatching)
        {
            candidates = GetPropertyDescriptors(inspectedProperties, container)
                .Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        else
        {
            candidates = GetPropertyDescriptors(inspectedProperties, container)
                .Where(p => p.Name == name);
        }

        using var enumerator = candidates.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            var ignoredProperty = inspectedProperties
                .Where(p => GetCustomAttribute<JsonIgnoreAttribute>(p)?.Condition is JsonIgnoreCondition.Always or JsonIgnoreCondition.WhenReading)
                .Select(p => GetCustomAttribute<JsonPropertyNameAttribute>(p)?.Name ?? p.Descriptor.Name)
                .FirstOrDefault(propertyName => caseInsensitivePropertyMatching
                    ? propertyName.Equals(name, StringComparison.OrdinalIgnoreCase)
                    : propertyName == name);

            if (ignoredProperty != null)
            {
                return null!;
            }

            var jsonExtensionData = inspectedProperties
                .FirstOrDefault(x => GetCustomAttribute<JsonExtensionDataAttribute>(x) != null);

            if (jsonExtensionData != null)
            {
                var prop = new ExtensionDataPropertyDescriptor(jsonExtensionData.Descriptor)
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
    private IReadOnlyList<InspectedProperty> GetInspectedProperties(Type type, object? container)
    {
        return _innerTypeDescriptor.GetProperties(type, container)
            .Select(property => new InspectedProperty(property, FindMostDerivedProperty(type, property.Name)))
            .ToArray();
    }

#if !NETSTANDARD2_0
    [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "ITypeInspector does not propagate DTO property preservation requirements. StaticContext registrations root public DTO properties.")]
    [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "ITypeInspector does not propagate DTO property preservation requirements. StaticContext registrations root public DTO properties.")]
#endif
    private static PropertyInfo? FindMostDerivedProperty(Type type, string name)
    {
        for (var currentType = type; currentType != null; currentType = currentType.BaseType)
        {
            var property = currentType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .FirstOrDefault(candidate => candidate.Name == name && candidate.GetIndexParameters().Length == 0);
            if (property != null)
            {
                return property;
            }
        }

        return null;
    }

    private static TAttribute? GetCustomAttribute<TAttribute>(InspectedProperty inspectedProperty)
        where TAttribute : Attribute
    {
        return inspectedProperty.Descriptor.GetCustomAttribute<TAttribute>()
            ?? inspectedProperty.ReflectedProperty?.GetCustomAttribute<TAttribute>();
    }

    private sealed class InspectedProperty
    {
        public InspectedProperty(IPropertyDescriptor descriptor, PropertyInfo? reflectedProperty)
        {
            Descriptor = descriptor;
            ReflectedProperty = reflectedProperty;
        }

        public IPropertyDescriptor Descriptor { get; }

        public PropertyInfo? ReflectedProperty { get; }
    }

    private sealed class EnumDefaultValuePropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor _inner;
        private readonly global::System.ComponentModel.DefaultValueAttribute _defaultValue;

        public EnumDefaultValuePropertyDescriptor(IPropertyDescriptor inner, global::System.ComponentModel.DefaultValueAttribute defaultValue)
        {
            _inner = inner;
            _defaultValue = defaultValue;
        }

        public bool AllowNulls => _inner.AllowNulls;

        public string Name => _inner.Name;

        public bool Required => _inner.Required;

        public Type Type => _inner.Type;

        public Type? TypeOverride { get => _inner.TypeOverride; set => _inner.TypeOverride = value; }

        public Type? ConverterType => _inner.ConverterType;

        public int Order { get => _inner.Order; set => _inner.Order = value; }

        public ScalarStyle ScalarStyle { get => _inner.ScalarStyle; set => _inner.ScalarStyle = value; }

        public bool CanWrite => _inner.CanWrite;

        public void Write(object target, object? value) => _inner.Write(target, value);

        public T? GetCustomAttribute<T>() where T : Attribute
        {
            return _inner.GetCustomAttribute<T>() ?? (_defaultValue is T attribute ? attribute : null);
        }

        public IObjectDescriptor Read(object target) => _inner.Read(target);
    }
}
