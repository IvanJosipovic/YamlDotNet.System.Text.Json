using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace TestFixtures;

/// <summary>Rich trim-safe model shared by integration and Native AOT tests.</summary>
public sealed class ComprehensiveConfiguration
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("display-name")]
    public string Name { get; set; } = "api";

    [JsonPropertyOrder(2)]
    [JsonPropertyName("port")]
    public int Port { get; set; } = 8080;

    [JsonPropertyName("mode")]
    public FixtureMode Mode { get; set; } = FixtureMode.Production;

    [JsonIgnore]
    public string InternalDescription { get; set; } = "internal";

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string AlwaysIgnored { get; set; } = "hidden";

    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public string NeverIgnored { get; set; } = "visible";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OptionalLabel { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int RetryLimit { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public string InputOnly { get; set; } = "input-default";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public string OutputOnly { get; set; } = "output-value";

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; } = new()
    {
        ["owner"] = "platform",
        ["future-setting"] = "enabled",
    };
}

public enum FixtureMode
{
    [JsonStringEnumMemberName("production")]
    Production,

    [JsonStringEnumMemberName("development")]
    Development,
}

/// <summary>Registered model used by tests for unmatched-property behavior.</summary>
public sealed class StaticApiProbe
{
    [JsonPropertyName("display-name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("mode")]
    public StaticApiProbeMode Mode { get; set; }
}

public enum StaticApiProbeMode
{
    [JsonStringEnumMemberName("production")]
    Production,
}

/// <summary>Test-only model fixtures organized by the capability they exercise.</summary>
public static class FixtureModels
{
    public static class ConfigurationAndAttributes
    {
        public sealed class PropertyNameModel
        {
            [JsonPropertyName("MyNewPropName")]
            public string MyProp { get; set; } = nameof(MyProp);

            public string MyProp2 { get; set; } = nameof(MyProp2);
        }

        public sealed class PropertyOrderModel
        {
            [JsonPropertyOrder(3)]
            public string MyProp { get; set; } = nameof(MyProp);

            [JsonPropertyOrder(2)]
            public string MyProp2 { get; set; } = nameof(MyProp2);

            [JsonPropertyOrder(1)]
            public string MyProp3 { get; set; } = nameof(MyProp3);
        }
    }

    public static class IgnoreConditions
    {
        public sealed class ConditionsModel
        {
            public string MyProp { get; set; } = nameof(MyProp);

            [JsonIgnore]
            public string Hide { get; set; } = nameof(Hide);

            [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
            public string Hide2 { get; set; } = nameof(Hide2);

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Show { get; set; } = nameof(Show);

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public string Show2 { get; set; } = nameof(Show2);

            [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
            public string Show3 { get; set; } = nameof(Show3);

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string? HideWhenNull { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string ShowWhenNotNull { get; set; } = nameof(ShowWhenNotNull);

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int HideWhenDefault { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int ShowWhenNotDefault { get; set; } = 42;

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public List<int> ShowWhenEmpty { get; set; } = [];

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public int? HideNullableWhenDefault { get; set; } = 0;
        }

        public sealed class DirectionalModel
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
            public string WriteOnly { get; set; } = nameof(WriteOnly);

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
            public string ReadOnly { get; set; } = nameof(ReadOnly);

            [JsonInclude]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
            public string IncludedWriteOnlyField = nameof(IncludedWriteOnlyField);

            [JsonInclude]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
            public string IncludedReadOnlyField = nameof(IncludedReadOnlyField);

            [JsonInclude]
            [JsonPropertyName("aliasedField")]
            [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
            public string IgnoredAliasedField = nameof(IgnoredAliasedField);

            [JsonInclude]
            [JsonPropertyName("extensionKey")]
            public string ExtensionKey = nameof(ExtensionKey);

            [JsonInclude]
            [JsonExtensionData]
            public Dictionary<string, object> ExtensionData = [];
        }
    }

    public static class Enums
    {
        public sealed class EnumModel
        {
            public TestEnum? Enum { get; set; }

            public TestEnum? EnumName { get; set; }

            public TestEnum? EnumNull { get; set; }

            public IList<TestEnum>? EnumList { get; set; }
        }

        public enum TestEnum
        {
            First,

            [JsonStringEnumMemberName("val2")]
            Second,

            Third,
        }

    }

    public static class ExtensionData
    {
        public sealed class JsonElementModel
        {
#pragma warning disable YDNG001 // JsonElement values are handled by SystemTextJsonYamlTypeConverter.
            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtensionData { get; set; }
#pragma warning restore YDNG001
        }

        public sealed class ObjectModel
        {
            [JsonExtensionData]
            public Dictionary<string, object>? ExtensionData { get; set; }
        }

        public sealed class MixedModel
        {
            public string? before { get; set; }

#pragma warning disable YDNG001 // JsonElement values are handled by SystemTextJsonYamlTypeConverter.
            [JsonExtensionData]
            public Dictionary<string, JsonElement>? ExtensionData { get; set; }
#pragma warning restore YDNG001

            public string? after { get; set; }
        }

        public sealed class ContainerWithJsonElementExtensionData
        {
#pragma warning disable YDNG001 // JsonElement values are handled by SystemTextJsonYamlTypeConverter.
            public IDictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
#pragma warning restore YDNG001
        }

        public static class DescriptorEdgeCases
        {
            public enum SignedEnum : long
            {
                Negative = -1,
            }

            public enum UnsignedEnum : ulong
            {
                Maximum = ulong.MaxValue,
            }

            public sealed class CustomDictionary : Dictionary<string, object>
            {
            }

            public sealed class CustomJsonElementDictionary : Dictionary<string, JsonElement>
            {
            }
        }
    }

    public static class DefaultValues
    {
        public sealed class Model
        {
            public string? NullableString { get; set; }

            public int Number { get; set; }

            public List<int> Numbers { get; set; } = new();
        }
    }

    public static class TypeInspectorInheritance
    {
        public class BaseHiddenPropertyModel
        {
            [JsonPropertyName("base-value")]
            public object Value { get; set; } = "base";
        }

        public sealed class DerivedHiddenPropertyModel : BaseHiddenPropertyModel
        {
            [JsonPropertyName("derived-value")]
            public new string Value { get; set; } = "derived";
        }

        public class BaseUnannotatedPropertyModel
        {
            public int Id { get; set; } = 1;
        }

        public sealed class DerivedUnannotatedPropertyModel : BaseUnannotatedPropertyModel
        {
            public new string Id { get; set; } = "derived";
        }
    }

    public static class UnmatchedProperties
    {
        public sealed class V1ObjectMeta
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = default!;
        }

        public sealed class V1CustomResourceDefinition
        {
            [JsonPropertyName("apiVersion")]
            public string ApiVersion { get; set; } = default!;

            [JsonPropertyName("kind")]
            public string Kind { get; set; } = default!;

            [JsonPropertyName("metadata")]
            public V1ObjectMeta Metadata { get; set; } = default!;
        }
    }
}

/// <summary>Builds and owns the disposable JSON document used by the canonical fixture.</summary>
public sealed class FixtureData : IDisposable
{
    public ComprehensiveConfiguration Model { get; }

    public JsonNode Node { get; }

    public JsonArray Array { get; }

    public JsonObject Object { get; }

    public JsonValue Value { get; }

    public JsonElement Element { get; }

    public JsonDocument Document { get; }

    public FixtureData()
    {
        Node = JsonNode.Parse("""{"enabled":true,"region":"west"}""")!;
        Array = JsonNode.Parse("""["blue",2,false]""")!.AsArray();
        Object = JsonNode.Parse("""{"owner":"platform","retries":3}""")!.AsObject();
        Value = JsonValue.Create("ready")!;
        Element = ParseElement("""{"endpoint":"https://example.test","healthy":true}""");
        Document = JsonDocument.Parse("""{"version":1,"items":["alpha","beta"]}""");
        Model = new ComprehensiveConfiguration();
    }

    public void Dispose()
    {
        Document.Dispose();
    }

    private static JsonElement ParseElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

[YamlStaticContext]
[YamlSerializable(typeof(ComprehensiveConfiguration))]
[YamlSerializable(typeof(FixtureMode))]
[YamlSerializable(typeof(StaticApiProbe))]
[YamlSerializable(typeof(StaticApiProbeMode))]
[YamlSerializable(typeof(FixtureModels.ConfigurationAndAttributes.PropertyNameModel))]
[YamlSerializable(typeof(FixtureModels.ConfigurationAndAttributes.PropertyOrderModel))]
[YamlSerializable(typeof(FixtureModels.IgnoreConditions.ConditionsModel))]
[YamlSerializable(typeof(FixtureModels.IgnoreConditions.DirectionalModel))]
[YamlSerializable(typeof(FixtureModels.Enums.EnumModel))]
[YamlSerializable(typeof(FixtureModels.Enums.TestEnum))]
[YamlSerializable(typeof(FixtureModels.ExtensionData.JsonElementModel))]
[YamlSerializable(typeof(FixtureModels.ExtensionData.ObjectModel))]
[YamlSerializable(typeof(FixtureModels.ExtensionData.MixedModel))]
[YamlSerializable(typeof(FixtureModels.ExtensionData.ContainerWithJsonElementExtensionData))]
[YamlSerializable(typeof(FixtureModels.ExtensionData.DescriptorEdgeCases.SignedEnum))]
[YamlSerializable(typeof(FixtureModels.ExtensionData.DescriptorEdgeCases.UnsignedEnum))]
[YamlSerializable(typeof(FixtureModels.DefaultValues.Model))]
[YamlSerializable(typeof(FixtureModels.TypeInspectorInheritance.BaseHiddenPropertyModel))]
[YamlSerializable(typeof(FixtureModels.TypeInspectorInheritance.DerivedHiddenPropertyModel))]
[YamlSerializable(typeof(FixtureModels.TypeInspectorInheritance.BaseUnannotatedPropertyModel))]
[YamlSerializable(typeof(FixtureModels.TypeInspectorInheritance.DerivedUnannotatedPropertyModel))]
[YamlSerializable(typeof(FixtureModels.UnmatchedProperties.V1ObjectMeta))]
[YamlSerializable(typeof(FixtureModels.UnmatchedProperties.V1CustomResourceDefinition))]
public partial class SharedYamlContext : StaticContext
{
}

/// <summary>Shared semantic assertions for sample and test round-trips.</summary>
public static class FixtureAssertions
{
    public static void Equivalent(
        ComprehensiveConfiguration expected,
        ComprehensiveConfiguration actual)
    {
        Assert(expected.Name == actual.Name, "The renamed property should round-trip.");
        Assert(expected.Port == actual.Port, "The ordered numeric property should round-trip.");
        Assert(expected.Mode == actual.Mode, "The enum and custom member name should round-trip.");
        Assert(actual.InternalDescription == "internal", "Always-ignored properties should retain their initializer.");
        Assert(actual.AlwaysIgnored == "hidden", "Always-ignored properties should retain their initializer.");
        Assert(actual.NeverIgnored == expected.NeverIgnored, "Never-ignored properties should round-trip.");
        Assert(actual.InputOnly == expected.InputOnly, "Write-only properties should remain readable from YAML.");
        Assert(actual.OutputOnly == "output-value", "Read-only properties should ignore YAML input.");
        Assert(actual.ExtensionData?["owner"] as string == "platform", "Object extension data should round-trip.");
        Assert(actual.ExtensionData?["future-setting"] as string == "enabled", "Additional object extension data should round-trip.");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
