using BenchmarkDotNet.Attributes;
using YamlDotNet.System.Text.Json;

namespace YamlBenchmarks;

/// <summary>
/// Compares the existing YamlDotNet reflection based APIs, the YamlDotNet static context APIs and
/// the new System.Text.Json contract based YamlSerializer.
/// </summary>
[MemoryDiagnoser]
public class SmallConfigBenchmarks
{
    private readonly SmallConfig _model = Fixtures.CreateSmallConfig();
    private readonly string _yaml = YamlSerializer.Serialize(Fixtures.CreateSmallConfig(), BenchmarkJsonContext.Default.SmallConfig);

    [Benchmark(Baseline = true)]
    public string YamlDotNetReflection_Serialize() => YamlConverter.Serialize(_model);

    [Benchmark]
    public string YamlDotNetStatic_Serialize() => YamlConverter.Serialize(_model, new BenchmarkYamlContext());

    [Benchmark]
    public string StjContracts_Serialize() => YamlSerializer.Serialize(_model, BenchmarkJsonContext.Default.SmallConfig);

    [Benchmark]
    public SmallConfig YamlDotNetReflection_Deserialize() => YamlConverter.Deserialize<SmallConfig>(_yaml)!;

    [Benchmark]
    public SmallConfig YamlDotNetStatic_Deserialize() => YamlConverter.Deserialize<SmallConfig>(_yaml, new BenchmarkYamlContext())!;

    [Benchmark]
    public SmallConfig? StjContracts_Deserialize() => YamlSerializer.Deserialize(_yaml, BenchmarkJsonContext.Default.SmallConfig);
}

[MemoryDiagnoser]
public class KubeManifestBenchmarks
{
    private readonly KubeManifest _model = Fixtures.CreateKubeManifest();
    private readonly string _yaml = YamlSerializer.Serialize(Fixtures.CreateKubeManifest(), BenchmarkJsonContext.Default.KubeManifest);

    [Benchmark(Baseline = true)]
    public string YamlDotNetReflection_Serialize() => YamlConverter.Serialize(_model);

    [Benchmark]
    public string YamlDotNetStatic_Serialize() => YamlConverter.Serialize(_model, new BenchmarkYamlContext());

    [Benchmark]
    public string StjContracts_Serialize() => YamlSerializer.Serialize(_model, BenchmarkJsonContext.Default.KubeManifest);

    [Benchmark]
    public KubeManifest YamlDotNetReflection_Deserialize() => YamlConverter.Deserialize<KubeManifest>(_yaml)!;

    [Benchmark]
    public KubeManifest YamlDotNetStatic_Deserialize() => YamlConverter.Deserialize<KubeManifest>(_yaml, new BenchmarkYamlContext())!;

    [Benchmark]
    public KubeManifest? StjContracts_Deserialize() => YamlSerializer.Deserialize(_yaml, BenchmarkJsonContext.Default.KubeManifest);
}

[MemoryDiagnoser]
public class ListBenchmarks
{
    private readonly ItemList _model = Fixtures.CreateItems(1000);
    private readonly string _yaml = YamlSerializer.Serialize(Fixtures.CreateItems(1000), BenchmarkJsonContext.Default.ItemList);

    [Benchmark(Baseline = true)]
    public string YamlDotNetReflection_Serialize() => YamlConverter.Serialize(_model);

    [Benchmark]
    public string YamlDotNetStatic_Serialize() => YamlConverter.Serialize(_model, new BenchmarkYamlContext());

    [Benchmark]
    public string StjContracts_Serialize() => YamlSerializer.Serialize(_model, BenchmarkJsonContext.Default.ItemList);

    [Benchmark]
    public ItemList YamlDotNetReflection_Deserialize() => YamlConverter.Deserialize<ItemList>(_yaml)!;

    [Benchmark]
    public ItemList YamlDotNetStatic_Deserialize() => YamlConverter.Deserialize<ItemList>(_yaml, new BenchmarkYamlContext())!;

    [Benchmark]
    public ItemList? StjContracts_Deserialize() => YamlSerializer.Deserialize(_yaml, BenchmarkJsonContext.Default.ItemList);
}

[MemoryDiagnoser]
public class LargeScalarBenchmarks
{
    private readonly LargeScalarDocument _model = Fixtures.CreateLargeScalar(64);
    private readonly string _yaml = YamlSerializer.Serialize(Fixtures.CreateLargeScalar(64), BenchmarkJsonContext.Default.LargeScalarDocument);

    [Benchmark(Baseline = true)]
    public string YamlDotNetReflection_Serialize() => YamlConverter.Serialize(_model);

    [Benchmark]
    public string YamlDotNetStatic_Serialize() => YamlConverter.Serialize(_model, new BenchmarkYamlContext());

    [Benchmark]
    public string StjContracts_Serialize() => YamlSerializer.Serialize(_model, BenchmarkJsonContext.Default.LargeScalarDocument);

    [Benchmark]
    public LargeScalarDocument YamlDotNetReflection_Deserialize() => YamlConverter.Deserialize<LargeScalarDocument>(_yaml)!;

    [Benchmark]
    public LargeScalarDocument YamlDotNetStatic_Deserialize() => YamlConverter.Deserialize<LargeScalarDocument>(_yaml, new BenchmarkYamlContext())!;

    [Benchmark]
    public LargeScalarDocument? StjContracts_Deserialize() => YamlSerializer.Deserialize(_yaml, BenchmarkJsonContext.Default.LargeScalarDocument);
}
