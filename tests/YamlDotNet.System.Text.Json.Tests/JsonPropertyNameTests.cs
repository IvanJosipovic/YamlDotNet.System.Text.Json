using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class JsonPropertyNameTests
{
    [Fact]
    public void Serialize()
    {
        var model = new FixtureModels.ConfigurationAndAttributes.PropertyNameModel();

        var yaml = StaticYaml.Serialize(model);

        var expected = """
                          MyNewPropName: MyProp
                          MyProp2: MyProp2

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void Deserialize()
    {
        var yaml = """
                    MyNewPropName: test
                    MyProp2: test2

                    """;

        var model = StaticYaml.Deserialize<FixtureModels.ConfigurationAndAttributes.PropertyNameModel>(yaml);

        model.MyProp.ShouldBe("test");
        model.MyProp2.ShouldBe("test2");
    }
}
