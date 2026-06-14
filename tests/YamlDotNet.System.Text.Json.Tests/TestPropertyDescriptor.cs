using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

internal sealed class TestPropertyDescriptor : IPropertyDescriptor
{
    private readonly IReadOnlyList<Attribute> _attributes;
    private readonly Func<object?, object?>? _reader;
    private readonly Action<object?, object?>? _writer;
    private object? _value;

    public TestPropertyDescriptor(
        string name,
        Type type,
        object? initialValue = null,
        IEnumerable<Attribute>? attributes = null,
        Func<object?, object?>? reader = null,
        Action<object?, object?>? writer = null)
    {
        Name = name;
        Type = type;
        _value = initialValue;
        _attributes = attributes?.ToArray() ?? Array.Empty<Attribute>();
        _reader = reader;
        _writer = writer;
    }

    public bool AllowNulls { get; set; }

    public string Name { get; set; }

    public bool Required => false;

    public Type Type { get; }

    public Type? TypeOverride { get; set; }

    public Type? ConverterType { get; set; }

    public int Order { get; set; }

    public ScalarStyle ScalarStyle { get; set; }

    public bool CanWrite { get; set; }

    public void Write(object? target, object? value)
    {
        _value = value;
        _writer?.Invoke(target, value);
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        foreach (var attribute in _attributes)
        {
            if (attribute is T typed)
            {
                return typed;
            }
        }

        return default;
    }

    public IObjectDescriptor Read(object? target)
    {
        var value = _reader?.Invoke(target) ?? _value;
        var type = value?.GetType() ?? typeof(object);
        return new ObjectDescriptor(value, type, type);
    }
}
