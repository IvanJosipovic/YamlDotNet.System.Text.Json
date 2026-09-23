using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

/// <summary>
/// Shows configuring YamlDotNet's static serializer and deserializer builders with this library.
/// POCO properties follow their configured property order; JSON object key sorting is shown in JsonInputExample.
/// </summary>
internal static class BuilderConfigurationExample
{
    /// <summary>Runs the direct static-builder serialization and deserialization example.</summary>
    public static void Run()
    {
        var context = new SamplesYamlContext();
        var model = new ServiceConfiguration { Name = "api", Port = 8080, Mode = ServiceMode.Production };
        var serializer = new StaticSerializerBuilder(context).AddSystemTextJson().Build();
        var deserializer = new StaticDeserializerBuilder(context).AddSystemTextJson().Build();

        var builderYaml = serializer.Serialize(model);
        SampleAssert.YamlEquals(builderYaml, """
            mode: production
            display-name: api
            port: 8080

            """, "Static builder configuration should preserve the DTO property order.");
        var restored = deserializer.Deserialize<ServiceConfiguration>(builderYaml);
        SampleAssert.That(restored.Name == model.Name, "Direct static-builder round-trip failed.");
    }
}
