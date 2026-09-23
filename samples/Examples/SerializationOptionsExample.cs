using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

/// <summary>
/// Shows POCO property ordering, default-value handling, and unmatched-property handling.
/// </summary>
internal static class SerializationOptionsExample
{
    /// <summary>Runs examples for POCO ordering, default omission, and unmatched properties.</summary>
    public static void Run()
    {
        var context = new SamplesYamlContext();
        var model = new ServiceConfiguration
        {
            Name = "api",
            Port = 8080,
            Mode = ServiceMode.Production,
        };

        var yamlWithJsonKeySortingEnabled = YamlConverter.Serialize(model, context, sortAlphabetically: true);
        SampleAssert.YamlEquals(yamlWithJsonKeySortingEnabled, """
            mode: production
            display-name: api
            port: 8080

            """, "sortAlphabetically applies to JSON object keys; POCO order follows YamlDotNet defaults and JsonPropertyOrder metadata.");

        var declarationOrderYaml = YamlConverter.Serialize(model, context, ignoreOrder: true);
        SampleAssert.YamlEquals(declarationOrderYaml, """
            display-name: api
            port: 8080
            mode: production

            """, "Ignoring JsonPropertyOrderAttribute should produce declaration-order YAML.");

        var defaultsYaml = YamlConverter.Serialize(new ServiceConfiguration(), context, defaultValuesHandling: DefaultValuesHandling.OmitDefaults);
        SampleAssert.YamlEquals(defaultsYaml, """
            display-name: ''

            """, "OmitDefaults should omit default enum and numeric values.");

        var forwardCompatibleYaml = YamlConverter.Serialize(model, context) + "future-setting: enabled\n";
        var forwardCompatible = YamlConverter.Deserialize<ServiceConfiguration>(forwardCompatibleYaml, context, ignoreUnmatchedProperties: true);
        SampleAssert.That(forwardCompatible.Name == model.Name, "Ignoring unmatched properties failed.");
    }
}
