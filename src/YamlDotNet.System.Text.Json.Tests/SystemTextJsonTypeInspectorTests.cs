using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace YamlDotNet.System.Text.Json.Tests;

public class SystemTextJsonTypeInspectorTests
{
    [Fact]
    public void GetProperty_UsesCaseInsensitiveMatching()
    {
        var descriptors = new[]
        {
            new TestPropertyDescriptor("ActualName", typeof(string))
        };

        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(descriptors));

        var result = inspector.GetProperty(typeof(object), null, "actualname", ignoreUnmatched: true, caseInsensitivePropertyMatching: true);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("ActualName");
    }

    [Fact]
    public void GetProperty_ReturnsExtensionDescriptorWhenMissing()
    {
        var container = new ContainerWithExtensionData();
        container.ExtensionData["dynamic"] = JsonSerializer.SerializeToElement("value");

        var attributes = new Attribute[] { new JsonExtensionDataAttribute() };
        var extensionDescriptor = new TestPropertyDescriptor(
            "ExtensionData",
            typeof(Dictionary<string, JsonElement>),
            reader: _ => container.ExtensionData,
            writer: (_, value) =>
            {
                if (value is IDictionary<string, JsonElement> dict)
                {
                    container.ExtensionData = dict;
                }
            },
            attributes: attributes);

        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(new[] { extensionDescriptor }));

        var result = inspector.GetProperty(typeof(ContainerWithExtensionData), container, "newKey", ignoreUnmatched: true, caseInsensitivePropertyMatching: false);

        result.ShouldBeOfType<ExtensionDataPropertyDescriptor>();
        result.Name.ShouldBe("newKey");
    }

    [Fact]
    public void GetProperty_ThrowsWhenMultipleMatches()
    {
        var descriptors = new[]
        {
            new TestPropertyDescriptor("Duplicate", typeof(string)),
            new TestPropertyDescriptor("Duplicate", typeof(string))
        };

        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(descriptors));

        Should.Throw<SerializationException>(() => inspector.GetProperty(typeof(object), null, "Duplicate", ignoreUnmatched: true, caseInsensitivePropertyMatching: false));
    }

    private sealed class StubTypeInspector : TypeInspectorSkeleton
    {
        private readonly IReadOnlyList<IPropertyDescriptor> _properties;

        public StubTypeInspector(IReadOnlyList<IPropertyDescriptor> properties)
        {
            _properties = properties;
        }

        public override string GetEnumName(Type enumType, string name) => name;

        public override string GetEnumValue(object enumValue) => enumValue.ToString()!;

        public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container) => _properties;

#pragma warning disable IDE0060 // Remove unused parameter
        public new IPropertyDescriptor GetProperty(Type type, object? container, string name, bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            if (caseInsensitivePropertyMatching)
            {
                return _properties.First(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            return _properties.First(p => p.Name == name);
        }

        public override bool HasParseMethod(Type type)
        {
            return false;
        }

        public override object? Parse(string value, Type expectedType)
        {
            throw new NotImplementedException();
        }
    }

    private sealed class ContainerWithExtensionData
    {
        public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
    }
}
