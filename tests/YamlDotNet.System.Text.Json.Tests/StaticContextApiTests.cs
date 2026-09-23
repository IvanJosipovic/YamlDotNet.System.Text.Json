using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

public class StaticContextApiTests
{
    [Fact]
    public void YamlConverterStaticContextOverloadsRoundTripDtoAndExtensionData()
    {
        var context = new TestYamlContext();
        var value = new StaticContextModel
        {
            Name = "api",
            Mode = StaticContextMode.Production,
            ExtensionData = new Dictionary<string, object>
            {
                ["owner"] = "platform",
            },
        };

        var yaml = YamlConverter.Serialize(value, context, sortAlphabetically: true);

        yaml.ReplaceLineEndings().ShouldBe("""
            display-name: api
            mode: production
            owner: platform

            """.ReplaceLineEndings());
        var restored = YamlConverter.Deserialize<StaticContextModel>(yaml, context);
        restored.Name.ShouldBe("api");
        restored.Mode.ShouldBe(StaticContextMode.Production);
        restored.ExtensionData!["owner"].ShouldBe("platform");
    }

    [Fact]
    public void BuilderExtensionsConfigureStaticBuilders()
    {
        var context = new TestYamlContext();
        var serializerBuilder = new StaticSerializerBuilder(context);
        serializerBuilder.AddSystemTextJson().ShouldBeSameAs(serializerBuilder);
        serializerBuilder.Build().ShouldNotBeNull();

        var deserializerBuilder = new StaticDeserializerBuilder(context);
        deserializerBuilder.AddSystemTextJson().ShouldBeSameAs(deserializerBuilder);
        deserializerBuilder.Build().ShouldNotBeNull();
    }

    [Fact]
    public void SerializeJsonUsesSuppliedDocumentOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            MaxDepth = 32,
        };

        YamlConverter.SerializeJson("""
            { // accepted by the supplied options
              "name": "api",
            }
            """, options).ShouldBe("name: api" + Environment.NewLine);
    }

    [Fact]
    public void ReflectionConvenienceOverloadRemainsAvailable()
    {
        var yaml = YamlConverter.Serialize((object)new StaticContextModel { Name = "api" });

        yaml.ShouldContain("display-name: api");
    }

    [Fact]
    public void StaticDeserializerCanIgnoreUnmatchedProperties()
    {
        var model = YamlConverter.Deserialize<StaticContextModel>("display-name: api\nunknown: value\n", new TestYamlContext(), ignoreUnmatchedProperties: true);

        model.Name.ShouldBe("api");
    }

}

public sealed class StaticContextModel
{
    [JsonPropertyName("display-name")]
    public string Name { get; set; } = string.Empty;

        [JsonPropertyName("mode")]
        public StaticContextMode Mode { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

public enum StaticContextMode
{
    [JsonStringEnumMemberName("production")]
    Production,
}

[YamlStaticContext]
[YamlSerializable(typeof(StaticContextModel))]
[YamlSerializable(typeof(StaticContextMode))]
public sealed partial class TestYamlContext : StaticContext
{
}
