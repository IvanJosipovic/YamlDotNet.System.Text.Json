using System.Text.Json;
using TestFixtures;

namespace YamlDotNet.System.Text.Json.Tests;

public class StaticContextApiTests
{
    [Fact]
    public void StaticContextOverloadsRejectNullContexts()
    {
        Should.Throw<ArgumentNullException>(() => YamlConverter.Serialize("value", null!))
            .ParamName.ShouldBe("context");
        Should.Throw<ArgumentNullException>(() => YamlConverter.SerializeJson("{}", (YamlDotNet.Serialization.StaticContext)null!))
            .ParamName.ShouldBe("context");
        Should.Throw<ArgumentNullException>(() => YamlConverter.Deserialize<string>("value", null!))
            .ParamName.ShouldBe("context");
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
            """, new SharedYamlContext(), options).ShouldBe("name: api" + Environment.NewLine);
    }

    [Fact]
    public void SerializeUsesSuppliedStaticContext()
    {
        var yaml = YamlConverter.Serialize(new ComprehensiveConfiguration { Name = "api" }, new SharedYamlContext());

        yaml.ShouldContain("display-name: api");
    }
}
