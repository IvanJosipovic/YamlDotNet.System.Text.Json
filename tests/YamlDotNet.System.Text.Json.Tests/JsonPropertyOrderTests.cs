using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class JsonPropertyOrderTests
{
    [Fact]
    public void PropertyOrder()
    {
        var model = new FixtureModels.ConfigurationAndAttributes.PropertyOrderModel();

        var yaml = StaticYaml.Serialize(model);

        var expected = """
                          MyProp3: MyProp3
                          MyProp2: MyProp2
                          MyProp: MyProp

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void DisablePropertyOrder()
    {
        var model = new FixtureModels.ConfigurationAndAttributes.PropertyOrderModel();

        var yaml = StaticYaml.Serialize(model, ignoreOrder: true);

        var expected = """
                          MyProp: MyProp
                          MyProp2: MyProp2
                          MyProp3: MyProp3

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }
}
