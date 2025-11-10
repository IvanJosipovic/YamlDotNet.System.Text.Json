using System.Text.Json;
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
        var (dict, type) = SystemTextJsonExtensionDataNodeDeserializer.GetOrCreateExtensionDataDictionary(target, _baseDescriptor);

        if (type == typeof(JsonElement))
        {
            dict[Name] = JsonSerializer.SerializeToElement(value);
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

    public IObjectDescriptor Read(object target)
    {
        var (dict, _) = SystemTextJsonExtensionDataNodeDeserializer.GetOrCreateExtensionDataDictionary(target, _baseDescriptor);

        var item = dict[Name];

        return new ObjectDescriptor(item, item?.GetType() ?? typeof(object), item?.GetType() ?? typeof(object));
    }
}
