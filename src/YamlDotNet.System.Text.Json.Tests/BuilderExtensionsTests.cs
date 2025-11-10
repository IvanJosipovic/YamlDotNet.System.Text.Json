using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class BuilderExtensionsTests
{
    [Fact]
    public void AddSystemTextJson_OnSerializerBuilder_ReturnsSameInstance()
    {
        var builder = new SerializerBuilder();

        var result = builder.AddSystemTextJson();

        result.ShouldBeSameAs(builder);

        var serializer = result.Build();
        serializer.ShouldNotBeNull();
    }

    [Fact]
    public void AddSystemTextJson_OnDeserializerBuilder_ReturnsSameInstance()
    {
        var builder = new DeserializerBuilder();

        var result = builder.AddSystemTextJson();

        result.ShouldBeSameAs(builder);

        var deserializer = result.Build();
        deserializer.ShouldNotBeNull();
    }

    [Fact]
    public void AddSystemTextJson_SerializerBuilderNull_ThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => BuilderExtensions.AddSystemTextJson((SerializerBuilder)null!));

        exception.ParamName.ShouldBe("builder");
    }

    [Fact]
    public void AddSystemTextJson_DeserializerBuilderNull_ThrowsArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => BuilderExtensions.AddSystemTextJson((DeserializerBuilder)null!));

        exception.ParamName.ShouldBe("builder");
    }

}
