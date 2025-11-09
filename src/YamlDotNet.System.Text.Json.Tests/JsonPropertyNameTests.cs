using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class JsonPropertyNameTests
{
    public class TestModel
    {
        [JsonPropertyName("MyNewPropName")]
        public string MyProp { get; set; }

        public string MyProp2 { get; set; }
    }

    [Fact]
    public void Serialize()
    {
        var model = new TestModel()
        {
            MyProp = nameof(TestModel.MyProp),
            MyProp2 = nameof(TestModel.MyProp2),
        };

        var yaml = YamlConverter.Serialize(model);

        string expected = """
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

        var model = YamlConverter.Deserialize<TestModel>(yaml);

        model.MyProp.ShouldBe("test");
        model.MyProp2.ShouldBe("test2");
    }
}
