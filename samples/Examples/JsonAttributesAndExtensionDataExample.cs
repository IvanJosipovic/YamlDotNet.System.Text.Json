using YamlDotNet.System.Text.Json;

/// <summary>
/// Shows how JSON property names, ordering, ignore rules, enum names, and extension data affect YAML output.
/// </summary>
internal static class JsonAttributesAndExtensionDataExample
{
    /// <summary>Runs examples for JSON property attributes and extension data.</summary>
    public static void Run()
    {
        var context = new SamplesYamlContext();
        var model = new ServiceConfiguration
        {
            Name = "api",
            Port = 8080,
            Mode = ServiceMode.Production,
            ExtensionData = new Dictionary<string, object>
            {
                ["owner"] = "platform",
            },
        };

        var yaml = YamlConverter.Serialize(model, context);
        SampleAssert.YamlEquals(yaml, """
            mode: production
            owner: platform
            display-name: api
            port: 8080

            """, "JSON attributes and extension data should produce the expected YAML document.");

        var restored = YamlConverter.Deserialize<ServiceConfiguration>(yaml, context);
        SampleAssert.That(restored.Name == model.Name && restored.Mode == model.Mode, "Convenience API round-trip failed.");
        SampleAssert.That(Equals(restored.ExtensionData?["owner"], "platform"), "Extension-data round-trip failed.");
    }
}
