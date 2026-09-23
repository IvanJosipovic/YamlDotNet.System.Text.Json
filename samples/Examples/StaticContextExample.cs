using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

/// <summary>
/// Shows the static-context convenience APIs and their equivalent direct YamlDotNet static-builder configuration.
/// </summary>
internal static class StaticContextExample
{
    /// <summary>Runs the static-context convenience API and direct-builder examples.</summary>
    public static void Run()
    {
        var context = new SamplesYamlContext();
        var model = new ServiceConfiguration
        {
            Name = "api",
            Port = 8080,
            Mode = ServiceMode.Production,
            ExtensionData = new Dictionary<string, object> { ["owner"] = "platform" },
        };

        var yaml = YamlConverter.Serialize(model, context, sortAlphabetically: true);
        SampleAssert.YamlEquals(yaml, """
            mode: production
            owner: platform
            display-name: api
            port: 8080

            """, "Static-context convenience APIs should produce the expected YAML document.");
        var restored = YamlConverter.Deserialize<ServiceConfiguration>(yaml, context);
        SampleAssert.That(restored.Name == model.Name, "Static-context round-trip failed.");
        SampleAssert.That(Equals(restored.ExtensionData?["owner"], "platform"), "Static-context extension data round-trip failed.");

        var serializer = new StaticSerializerBuilder(context).AddSystemTextJson().Build();
        var deserializer = new StaticDeserializerBuilder(context).AddSystemTextJson().Build();
        var directYaml = serializer.Serialize(model);
        SampleAssert.YamlEquals(directYaml, """
            mode: production
            owner: platform
            display-name: api
            port: 8080

            """, "Direct static builder configuration should produce the expected YAML document.");
        var directRoundTrip = deserializer.Deserialize<ServiceConfiguration>(directYaml);
        SampleAssert.That(directRoundTrip.Name == model.Name, "Direct static-builder round-trip failed.");

    }
}
