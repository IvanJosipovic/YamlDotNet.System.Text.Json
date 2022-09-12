using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.System.Text.Json;

/// <summary>
/// Applies property settings from <see cref="JsonPropertyNameAttribute"/> and <see cref="JsonIgnoreAttribute"/> to YamlDotNet
/// </summary>
public sealed class SystemTextJsonTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector innerTypeDescriptor;

    public SystemTextJsonTypeInspector(ITypeInspector innerTypeDescriptor)
    {
        this.innerTypeDescriptor = innerTypeDescriptor;
    }

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
    {
        return innerTypeDescriptor.GetProperties(type, container)
            .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null ||
                        p.GetCustomAttribute<JsonIgnoreAttribute>().Condition == JsonIgnoreCondition.Never ||
                        p.GetCustomAttribute<JsonIgnoreAttribute>().Condition != JsonIgnoreCondition.Always
            )
            .Select(p =>
            {
                var descriptor = new PropertyDescriptor(p);

                var nameAttribute = p.GetCustomAttribute<JsonPropertyNameAttribute>();

                if (nameAttribute != null)
                {
                    descriptor.Name = nameAttribute.Name;
                }

                var orderAttribute = p.GetCustomAttribute<JsonPropertyOrderAttribute>();

                if (orderAttribute != null)
                {
                    descriptor.Order = orderAttribute.Order;
                }

                return (IPropertyDescriptor)descriptor;
            })
            .OrderBy(p => p.Order);
    }
}