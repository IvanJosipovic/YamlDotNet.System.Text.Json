using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

/// <summary>
/// Shows converting JSON text to YAML with a trim-safe static serializer and JSON parsing options.
/// </summary>
internal static class JsonInputExample
{
    /// <summary>Runs the JSON document to YAML example using sorted object keys.</summary>
    public static void Run()
    {
        using var document = JsonDocument.Parse(
            "{\"z\":1,\"a\":2,}",
            new JsonDocumentOptions { AllowTrailingCommas = true });
        var serializer = new StaticSerializerBuilder(new SamplesYamlContext())
            .AddSystemTextJson(sortAlphabetically: true)
            .Build();
        var yaml = serializer.Serialize(document);

        SampleAssert.YamlEquals(yaml, """
            a: 2
            z: 1

            """, "The static builder should serialize the parsed JSON document in key order.");
    }
}
