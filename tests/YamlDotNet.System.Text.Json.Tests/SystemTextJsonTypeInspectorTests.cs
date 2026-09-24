using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

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
        var container = new FixtureModels.ExtensionData.ContainerWithJsonElementExtensionData();
        container.ExtensionData["dynamic"] = SharedJson.ParseElement("\"value\"");

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

        var result = inspector.GetProperty(typeof(FixtureModels.ExtensionData.ContainerWithJsonElementExtensionData), container, "newKey", ignoreUnmatched: true, caseInsensitivePropertyMatching: false);

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

    [Fact]
    public void GetPropertiesThrowsForUnsupportedJsonIgnoreCondition()
    {
        var descriptor = new TestPropertyDescriptor(
            "Value",
            typeof(string),
            attributes: new Attribute[] { new JsonIgnoreAttribute { Condition = (JsonIgnoreCondition)999 } });
        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(new[] { descriptor }));

        Should.Throw<ArgumentOutOfRangeException>(() => inspector.GetProperties(typeof(object), null).ToArray());
    }

    [Fact]
    public void ParseMetadataMethodsReportNoCustomParser()
    {
        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(Array.Empty<IPropertyDescriptor>()));

        inspector.HasParseMethod(typeof(string)).ShouldBeFalse();
        Should.Throw<NotImplementedException>(() => inspector.Parse("value", typeof(string)));
    }

    [Fact]
    public void GetPropertiesUsesTheMostDerivedHiddenPropertyAttribute()
    {
        var descriptor = new TestPropertyDescriptor("Value", typeof(string));
        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(new[] { descriptor }));

        var property = inspector.GetProperties(
            typeof(FixtureModels.TypeInspectorInheritance.DerivedHiddenPropertyModel),
            new FixtureModels.TypeInspectorInheritance.DerivedHiddenPropertyModel()).Single();

        property.Name.ShouldBe("derived-value");
    }

    [Fact]
    public void GetPropertiesIncludesOnlyJsonIncludedFields()
    {
        var descriptors = new IPropertyDescriptor[]
        {
            new TestPropertyDescriptor(nameof(FixtureModels.ConfigurationAndAttributes.FieldModel.UnannotatedField), typeof(string)),
            new TestPropertyDescriptor(nameof(FixtureModels.ConfigurationAndAttributes.FieldModel.IncludedField), typeof(string)),
        };
        var inspector = new SystemTextJsonTypeInspector(new StubTypeInspector(descriptors));

        var properties = inspector.GetProperties(
            typeof(FixtureModels.ConfigurationAndAttributes.FieldModel),
            new FixtureModels.ConfigurationAndAttributes.FieldModel()).ToArray();

        properties.Select(property => property.Name).ShouldBe(new[] { "included-field" });
    }

    [Fact]
    public void SerializerHandlesUnannotatedHiddenProperties()
    {
        var yaml = StaticYaml.Serialize(new FixtureModels.TypeInspectorInheritance.DerivedHiddenPropertyModel());

        yaml.ShouldContain("derived-value: derived");
        Should.NotThrow(() => StaticYaml.Serialize(new FixtureModels.TypeInspectorInheritance.DerivedUnannotatedPropertyModel()));
    }

    private sealed class StubTypeInspector : ITypeInspector
    {
        private readonly IReadOnlyList<IPropertyDescriptor> _properties;

        public StubTypeInspector(IReadOnlyList<IPropertyDescriptor> properties)
        {
            _properties = properties;
        }

        public string GetEnumName(Type enumType, string name) => name;

        public string GetEnumValue(object enumValue) => enumValue.ToString()!;

        public IEnumerable<IPropertyDescriptor> GetProperties(Type type, object? container) => _properties;

        public IPropertyDescriptor GetProperty(Type type, object? container, string name, bool ignoreUnmatched, bool caseInsensitivePropertyMatching)
        {
            if (caseInsensitivePropertyMatching)
            {
                return _properties.First(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }

            return _properties.First(p => p.Name == name);
        }

        public bool HasParseMethod(Type type)
        {
            return false;
        }

        public object? Parse(string value, Type expectedType)
        {
            throw new NotImplementedException();
        }
    }

}
