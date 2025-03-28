﻿using System.Reflection;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Applies property settings from <see cref="JsonPropertyNameAttribute"/> and <see cref="JsonIgnoreAttribute"/> and <see cref="JsonStringEnumMemberNameAttribute"/> to YamlDotNet
/// </summary>
public sealed class SystemTextJsonTypeInspector : TypeInspectorSkeleton
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

    public override string GetEnumName(Type enumType, string name)
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

    public override string GetEnumValue(object enumValue)
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

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
    {
        return innerTypeDescriptor.GetProperties(type, container)
            .Where(p =>
            {
                var attr = p.GetCustomAttribute<JsonIgnoreAttribute>();

                return attr == null ||
                       attr.Condition == JsonIgnoreCondition.Never ||
                       attr.Condition != JsonIgnoreCondition.Always;
            }
            )
            .Select(p =>
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

                return (IPropertyDescriptor)descriptor;
            })
            .OrderBy(p => p.Order);
    }
}