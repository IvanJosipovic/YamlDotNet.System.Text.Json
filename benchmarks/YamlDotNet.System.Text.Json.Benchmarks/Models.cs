using System.Text.Json.Serialization;

namespace YamlBenchmarks;

public sealed class SmallConfig
{
    public string Name { get; set; } = "";
    public int Replicas { get; set; }
    public bool Enabled { get; set; }
    public string Description { get; set; } = "";
    public List<string> Tags { get; set; } = [];
    public Dictionary<string, string> Labels { get; set; } = [];
}

public sealed class Container
{
    public string Name { get; set; } = "";
    public string Image { get; set; } = "";
    public List<string> Args { get; set; } = [];
    public Dictionary<string, string> Env { get; set; } = [];
    public int CpuLimit { get; set; }
    public int MemoryLimitMb { get; set; }
}

public sealed class PodSpec
{
    public string RestartPolicy { get; set; } = "";
    public List<Container> Containers { get; set; } = [];
}

public sealed class KubeManifest
{
    public string ApiVersion { get; set; } = "";
    public string Kind { get; set; } = "";
    public Dictionary<string, string> Metadata { get; set; } = [];
    public PodSpec Spec { get; set; } = new();
}

public sealed class Item
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public double Weight { get; set; }
    public bool Active { get; set; }
    public List<string> Values { get; set; } = [];
}

public sealed class ItemList
{
    public List<Item> Items { get; set; } = [];
}

public sealed class LargeScalarDocument
{
    public string Content { get; set; } = "";
}

[JsonSerializable(typeof(SmallConfig))]
[JsonSerializable(typeof(KubeManifest))]
[JsonSerializable(typeof(ItemList))]
[JsonSerializable(typeof(LargeScalarDocument))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class BenchmarkJsonContext : JsonSerializerContext;
