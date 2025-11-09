using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class JsonPropertyOrderTests
{
    public class TestModel2
    {
        [JsonPropertyOrder(3)]
        public string MyProp { get; set; } = nameof(MyProp);

        [JsonPropertyOrder(2)]
        public string MyProp2 { get; set; } = nameof(MyProp2);

        [JsonPropertyOrder(1)]
        public string MyProp3 { get; set; } = nameof(MyProp3);
    }

    [Fact]
    public void PropertyOrder()
    {
        var model = new TestModel2();

        var yaml = YamlConverter.Serialize(model);

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
        var model = new TestModel2();

        var yaml = YamlConverter.Serialize(model, ignoreOrder: true);

        var expected = """
                          MyProp: MyProp
                          MyProp2: MyProp2
                          MyProp3: MyProp3

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }
}
