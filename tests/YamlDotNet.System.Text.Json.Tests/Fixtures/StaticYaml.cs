using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using TestFixtures;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

internal static class StaticYaml
{
    private static readonly SharedYamlContext Context = new();

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(ComprehensiveConfiguration))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicFields, typeof(FixtureMode))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicProperties, typeof(StaticApiProbe))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicFields, typeof(StaticApiProbeMode))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ConfigurationAndAttributes.PropertyNameModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ConfigurationAndAttributes.PropertyOrderModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ConfigurationAndAttributes.FieldModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.IgnoreConditions.ConditionsModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.IgnoreConditions.DirectionalModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.Enums.EnumModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicFields, typeof(FixtureModels.Enums.TestEnum))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ExtensionData.JsonElementModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ExtensionData.ObjectModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ExtensionData.MixedModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ExtensionData.ContainerWithJsonElementExtensionData))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ExtensionData.DescriptorEdgeCases.CustomDictionary))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.ExtensionData.DescriptorEdgeCases.CustomJsonElementDictionary))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.DefaultValues.Model))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.TypeInspectorInheritance.BaseHiddenPropertyModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.TypeInspectorInheritance.DerivedHiddenPropertyModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.TypeInspectorInheritance.BaseUnannotatedPropertyModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.TypeInspectorInheritance.DerivedUnannotatedPropertyModel))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.UnmatchedProperties.V1ObjectMeta))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FixtureModels.UnmatchedProperties.V1CustomResourceDefinition))]
    static StaticYaml()
    {
    }

    public static string Serialize<T>(
        T value,
        bool sortAlphabetically = false,
        bool ignoreOrder = false,
        DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        return YamlConverter.Serialize(value, Context, sortAlphabetically, ignoreOrder, defaultValuesHandling);
    }

    public static T Deserialize<T>(string yaml, bool ignoreUnmatchedProperties = false)
    {
        return YamlConverter.Deserialize<T>(yaml, Context, ignoreUnmatchedProperties);
    }

    public static string SerializeJson(
        string json,
        JsonSerializerOptions? jsonSerializerOptions = null,
        bool sortAlphabetically = false,
        bool ignoreOrder = false,
        DefaultValuesHandling defaultValuesHandling = DefaultValuesHandling.Preserve)
    {
        return YamlConverter.SerializeJson(
            json,
            Context,
            jsonSerializerOptions,
            sortAlphabetically,
            ignoreOrder,
            defaultValuesHandling);
    }
}

internal static class SharedJson
{
    public static JsonElement ParseElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
