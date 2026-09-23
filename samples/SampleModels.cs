using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

internal sealed class ServiceConfiguration
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("display-name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyOrder(2)]
    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("mode")]
    public ServiceMode Mode { get; set; }

    [JsonIgnore]
    public string InternalDescription { get; set; } = "not serialized";

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

internal enum ServiceMode
{
    [JsonStringEnumMemberName("production")]
    Production,

    [JsonStringEnumMemberName("development")]
    Development,
}

[YamlStaticContext]
[YamlSerializable(typeof(ServiceConfiguration))]
[YamlSerializable(typeof(ServiceMode))]
public partial class SamplesYamlContext : StaticContext
{
}
