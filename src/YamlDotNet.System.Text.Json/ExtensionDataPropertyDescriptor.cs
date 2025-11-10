using System.Collections;
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
        var (dict, type) = GetOrCreateExtensionDataDictionary(target, _baseDescriptor);

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
        var (dict, _) = GetOrCreateExtensionDataDictionary(target, _baseDescriptor);

        var item = dict[Name];

        return new ObjectDescriptor(item, item?.GetType() ?? typeof(object), item?.GetType() ?? typeof(object));
    }

    private static (Type key, Type val)? GetIDictionaryKVTypes(Type t)
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

    private static (IDictionary, Type) GetOrCreateExtensionDataDictionary(object target, IPropertyDescriptor prop)
    {
        var (_, val) = GetIDictionaryKVTypes(prop.Type)
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

        throw new InvalidOperationException($"Extension data property must be of type IDictionary<string, object> or IDictionary<string, JsonElement>. Found: {val.FullName}.");
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
