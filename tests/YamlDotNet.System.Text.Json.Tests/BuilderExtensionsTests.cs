using TestFixtures;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class BuilderExtensionsTests
{
    [Fact]
    public void AddSystemTextJson_OnStaticSerializerBuilder_ReturnsSameInstance()
    {
        var builder = new StaticSerializerBuilder(new SharedYamlContext());

        var result = builder.AddSystemTextJson();

        result.ShouldBeSameAs(builder);

        var serializer = result.Build();
        serializer.ShouldNotBeNull();
    }

    [Fact]
    public void AddSystemTextJson_OnStaticDeserializerBuilder_ReturnsSameInstance()
    {
        var builder = new StaticDeserializerBuilder(new SharedYamlContext());

        var result = builder.AddSystemTextJson();

        result.ShouldBeSameAs(builder);

        var deserializer = result.Build();
        deserializer.ShouldNotBeNull();
    }

    [Fact]
    public void AddSystemTextJson_StaticSerializerBuilderNull_ThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => BuilderExtensions.AddSystemTextJson((StaticSerializerBuilder)null!));

        exception.ParamName.ShouldBe("builder");
    }

    [Fact]
    public void AddSystemTextJson_StaticDeserializerBuilderNull_ThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => BuilderExtensions.AddSystemTextJson((StaticDeserializerBuilder)null!));

        exception.ParamName.ShouldBe("builder");
    }

}
