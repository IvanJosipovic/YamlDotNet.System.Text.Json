using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TestFixtures;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

[CollectionDefinition("Reflection API tests", DisableParallelization = true)]
public sealed class ReflectionApiCollection
{
    public const string Name = "Reflection API tests";
}

[Collection(ReflectionApiCollection.Name)]
public class ReflectionApiTests
{
    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The reflection APIs are explicitly trim-unsafe and this test runs only when dynamic code is supported.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "The reflection APIs are explicitly unsupported for Native AOT and this test runs only when dynamic code is supported.")]
    public void ReflectionConvenienceApisWorkWhenDynamicCodeIsSupported()
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            return;
        }

        var model = new ComprehensiveConfiguration { Name = "reflection" };

        YamlConverter.Serialize((object)model).ShouldContain("display-name: reflection");
        YamlConverter.Serialize(model).ShouldContain("display-name: reflection");
        YamlConverter.SerializeJson("{\"name\":\"reflection\"}").ShouldContain("name: reflection");
        YamlConverter.SerializeJson("{\"name\":\"reflection\",}", new JsonSerializerOptions { AllowTrailingCommas = true })
            .ShouldContain("name: reflection");

        var yaml = YamlConverter.Serialize(model);
        YamlConverter.Deserialize<ComprehensiveConfiguration>(yaml).Name.ShouldBe("reflection");
        Should.Throw<YamlDotNet.Core.YamlException>(() => YamlConverter.Deserialize<FixtureModels.ConfigurationAndAttributes.PropertyNameModel>("unknown: value"));
        YamlConverter.Deserialize<FixtureModels.ConfigurationAndAttributes.PropertyNameModel>(
            "unknown: value", ignoreUnmatchedProperties: true).MyProp.ShouldBe("MyProp");
    }

    [Fact]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The reflection builders are explicitly trim-unsafe and this test runs only when dynamic code is supported.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "The reflection builders are explicitly unsupported for Native AOT and this test runs only when dynamic code is supported.")]
    public void ReflectionBuilderExtensionsConfigureJsonHandlingWhenDynamicCodeIsSupported()
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            return;
        }

        var serializer = new SerializerBuilder().AddSystemTextJson().Build();
        serializer.Serialize(new ComprehensiveConfiguration { Name = "builder" }).ShouldContain("display-name: builder");

        var deserializer = new DeserializerBuilder().AddSystemTextJson().Build();
        deserializer.Deserialize<ComprehensiveConfiguration>("display-name: builder").Name.ShouldBe("builder");
    }
}
