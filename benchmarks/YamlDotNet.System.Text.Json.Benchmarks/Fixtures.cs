using System.Text;
using YamlDotNet.Serialization;

namespace YamlBenchmarks;

[YamlStaticContext]
[YamlSerializable(typeof(SmallConfig))]
[YamlSerializable(typeof(KubeManifest))]
[YamlSerializable(typeof(PodSpec))]
[YamlSerializable(typeof(Container))]
[YamlSerializable(typeof(Item))]
[YamlSerializable(typeof(ItemList))]
[YamlSerializable(typeof(LargeScalarDocument))]
public partial class BenchmarkYamlContext : StaticContext
{
}

public static class Fixtures
{
    public static SmallConfig CreateSmallConfig()
    {
        return new SmallConfig
        {
            Name = "api",
            Replicas = 3,
            Enabled = true,
            Description = "A small configuration object",
            Tags = ["a", "b", "c"],
            Labels =
            {
                ["app"] = "api",
                ["tier"] = "backend",
            },
        };
    }

    public static KubeManifest CreateKubeManifest()
    {
        var manifest = new KubeManifest
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata =
            {
                ["name"] = "api",
                ["namespace"] = "default",
                ["app.kubernetes.io/name"] = "api",
                ["app.kubernetes.io/instance"] = "api-prod",
            },
            Spec = new PodSpec
            {
                RestartPolicy = "Always",
            },
        };

        for (var i = 0; i < 10; i++)
        {
            manifest.Spec.Containers.Add(new Container
            {
                Name = $"container-{i}",
                Image = $"registry.example.com/api:v1.{i}.0",
                Args = ["--port", "8080", "--enable-metrics"],
                Env =
                {
                    ["LOG_LEVEL"] = "info",
                    ["SERVICE_NAME"] = $"container-{i}",
                },
                CpuLimit = 2,
                MemoryLimitMb = 512,
            });
        }

        return manifest;
    }

    public static ItemList CreateItems(int count)
    {
        var items = new List<Item>(count);

        for (var i = 0; i < count; i++)
        {
            items.Add(new Item
            {
                Id = i,
                Title = $"Item {i}",
                Weight = 0.5 + (i % 10) * 0.1,
                Active = i % 2 == 0,
                Values = [$"value-{i}-a", $"value-{i}-b", $"value-{i}-c"],
            });
        }

        return new ItemList { Items = items };
    }

    public static LargeScalarDocument CreateLargeScalar(int sizeInKilobytes)
    {
        var line = "The quick brown fox jumps over the lazy dog. ";
        var content = new StringBuilder(sizeInKilobytes * 1024);

        while (content.Length < sizeInKilobytes * 1024)
        {
            content.Append(line);
        }

        return new LargeScalarDocument { Content = content.ToString() };
    }
}
