using System.Text.Json;
using ExternalContracts;

namespace YamlDotNet.System.Text.Json.Tests;

/// <summary>
/// Verifies that YamlSerializer works with a JsonSerializerContext generated in a separate assembly,
/// without modifying or regenerating that context.
/// </summary>
public class ExternalContextTests
{
    [Fact]
    public void UsesExternalSourceGeneratedContextForSerialization()
    {
        var model = new ExternalModel
        {
            Name = "api",
            Replicas = 3,
            Description = "external contract",
        };

        var yaml = YamlSerializer.Serialize(model, ExternalJsonContext.Default.ExternalModel);

        yaml.ShouldContain("display-name: api");
        yaml.ShouldContain("Replicas: 3");
        yaml.ShouldContain("Description: external contract");
    }

    [Fact]
    public void UsesExternalSourceGeneratedContextForDeserialization()
    {
        var yaml = """
            display-name: api
            Replicas: 3
            Description: external contract
            """;

        var result = YamlSerializer.Deserialize(yaml, ExternalJsonContext.Default.ExternalModel);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("api");
        result.Replicas.ShouldBe(3);
        result.Description.ShouldBe("external contract");
    }

    [Fact]
    public void ExternalContextRoundTripsThroughBothFormats()
    {
        var model = new ExternalModel
        {
            Name = "api",
            Replicas = 3,
            Description = "external contract",
        };

        var json = JsonSerializer.Serialize(model, ExternalJsonContext.Default.ExternalModel);
        var yaml = YamlSerializer.Serialize(model, ExternalJsonContext.Default.ExternalModel);

        var fromJson = JsonSerializer.Deserialize(json, ExternalJsonContext.Default.ExternalModel);
        var fromYaml = YamlSerializer.Deserialize(yaml, ExternalJsonContext.Default.ExternalModel);

        fromJson.ShouldNotBeNull();
        fromYaml.ShouldNotBeNull();
        fromYaml.Name.ShouldBe(fromJson.Name);
        fromYaml.Replicas.ShouldBe(fromJson.Replicas);
        fromYaml.Description.ShouldBe(fromJson.Description);
    }

    [Fact]
    public void ExternalContextSupportsCollections()
    {
        var models = new List<ExternalModel>
        {
            new() { Name = "one", Replicas = 1 },
            new() { Name = "two", Replicas = 2 },
        };

        var yaml = YamlSerializer.Serialize(models, ExternalJsonContext.Default.ListExternalModel);
        var result = YamlSerializer.Deserialize(yaml, ExternalJsonContext.Default.ListExternalModel);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("one");
        result[1].Replicas.ShouldBe(2);
    }
}
