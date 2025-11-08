using System.Reflection;
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
    private readonly ITypeInspector innerTypeDescriptor;

    private readonly bool ignoreOrder;

    /// <summary>
    /// Applies property settings from <see cref="JsonPropertyNameAttribute"/> and <see cref="JsonIgnoreAttribute"/> and <see cref="JsonStringEnumMemberNameAttribute"/> to YamlDotNet
    /// </summary>
    /// <param name="innerTypeDescriptor"></param>
    /// <param name="ignoreOrder">ignores JsonPropertyOrder</param>
    public SystemTextJsonTypeInspector(ITypeInspector innerTypeDescriptor, bool ignoreOrder = false)
    {
        this.innerTypeDescriptor = innerTypeDescriptor;
        this.ignoreOrder = ignoreOrder;
    }

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

        return innerTypeDescriptor.GetEnumName(enumType, name);
    }

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

        return innerTypeDescriptor.GetEnumValue(enumValue);
    }

    public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container)
    {
        // First, process declared properties as before.
        var declaredProperties = innerTypeDescriptor.GetProperties(type, container)
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
                    var props = new List<IPropertyDescriptor>();

                    if (container == null)
                    {
                        var descriptor = new PropertyDescriptor(p);
                        return [descriptor];
                    }
                    else
                    {
                        if (p.Read(container).Value is IDictionary<string, JsonElement> extData)
                        {
                            foreach (var entry in extData)
                            {
                                // Create a property descriptor for each extension data key/value.
                                var extProp = new ExtensionDataPropertyDescriptor(entry.Key, entry.Value);
                                props.Add(extProp);
                            }
                        }
                        else if (p.Read(container).Value is IDictionary<string, object> extData2)
                        {
                            foreach (var entry in extData2)
                            {
                                // Create a property descriptor for each extension data key/value.
                                var extProp = new ExtensionDataPropertyDescriptor(entry.Key, entry.Value);
                                props.Add(extProp);
                            }
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

                    if (!ignoreOrder)
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
        if (ignoreOrder)
        {
            return declaredProperties;
        }
        return declaredProperties.OrderBy(p => p.Order);
    }

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
            if (ignoreUnmatched)
            {
                return null!;
            }

            var jsonExtensionData = GetProperties(type, container).FirstOrDefault(x => x.GetCustomAttribute<JsonExtensionDataAttribute>() != null);

            if (jsonExtensionData != null)
            {
                // This means we need to support arbitrary properties
                return jsonExtensionData;
            }


            throw new SerializationException($"Property '{name}' not found on type '{type.FullName}'.");
        }

        var property = enumerator.Current;

        if (enumerator.MoveNext())
        {
            throw new SerializationException(
                $"Multiple properties with the name/alias '{name}' already exists on type '{type.FullName}', maybe you're misusing YamlAlias or maybe you are using the wrong naming convention? The matching properties are: {string.Join(", ", candidates.Select(p => p.Name).ToArray())}"
            );
        }

        return property;
    }
}

public class ExtensionDataPropertyDescriptor : IPropertyDescriptor
{
    private readonly string name;
    private readonly object? value;

    // Use a high order so extension data comes after defined properties.
    public ExtensionDataPropertyDescriptor(string name, object? value)
    {
        this.name = name;
        this.value = value;
        Order = int.MaxValue;
        ScalarStyle = ScalarStyle.Any;
    }

    public string Name
    {
        get => name;
        set { /* read-only */ }
    }

    public Type Type => value?.GetType() ?? typeof(object);

    public int Order { get; set; }

    public ScalarStyle ScalarStyle { get; set; }

    public bool CanWrite => false;

    public bool AllowNulls => true;

    public Type? TypeOverride { get; set; }

    public bool Required => false;

    public Type? ConverterType => null;

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        return null;
    }

    public IObjectDescriptor Read(object target)
    {
        return new ObjectDescriptor(value, Type, Type);
    }

    public void Write(object target, object value)
    {
        // Writing not supported.
    }
}
