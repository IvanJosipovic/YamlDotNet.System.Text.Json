using System.Text.Json.Serialization;

namespace ExternalContracts;

/// <summary>
/// A source generated System.Text.Json context owned by a separate assembly, simulating a third
/// party contract. It must work with <c>YamlSerializer</c> without modification or regeneration.
/// </summary>
[JsonSerializable(typeof(ExternalModel))]
[JsonSerializable(typeof(List<ExternalModel>))]
public partial class ExternalJsonContext : JsonSerializerContext
{
}

public sealed class ExternalModel
{
    [JsonPropertyName("display-name")]
    public string Name { get; set; } = string.Empty;

    public int Replicas { get; set; }

    public string? Description { get; set; }
}
