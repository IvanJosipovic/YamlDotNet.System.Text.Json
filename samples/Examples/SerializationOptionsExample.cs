using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

/// <summary>
/// Shows alphabetical sorting, ignoring JSON order attributes, default-value handling, and unmatched-property handling.
/// </summary>
internal static class SerializationOptionsExample
{
    public static void Run()
    {
        var context = new SamplesYamlContext();
        var model = new ServiceConfiguration
        {
            Name = "api",
            Port = 8080,
            Mode = ServiceMode.Production,
        };

        var sortedYaml = YamlConverter.Serialize(model, context, sortAlphabetically: true);
        SampleAssert.YamlEquals(sortedYaml, """
            mode: production
            display-name: api
            port: 8080

            """, "Alphabetical ordering should produce the expected YAML document.");

        var declarationOrderYaml = YamlConverter.Serialize(model, context, ignoreOrder: true);
        SampleAssert.YamlEquals(declarationOrderYaml, """
            display-name: api
            port: 8080
            mode: production

            """, "Ignoring JsonPropertyOrderAttribute should produce declaration-order YAML.");

        var defaultsYaml = YamlConverter.Serialize(new ServiceConfiguration(), context, defaultValuesHandling: DefaultValuesHandling.OmitDefaults);
        SampleAssert.YamlEquals(defaultsYaml, """
            mode: production
            display-name: ''

            """, "OmitDefaults should produce the expected YAML document.");

        var forwardCompatibleYaml = YamlConverter.Serialize(model, context) + "future-setting: enabled\n";
        var forwardCompatible = YamlConverter.Deserialize<ServiceConfiguration>(forwardCompatibleYaml, context, ignoreUnmatchedProperties: true);
        SampleAssert.That(forwardCompatible.Name == model.Name, "Ignoring unmatched properties failed.");
    }
}
