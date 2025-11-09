using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class JsonPropertyOrderTests
{
    public class TestModel2
    {
        [JsonPropertyOrder(3)]
        public string MyProp { get; set; }

        [JsonPropertyOrder(2)]
        public string MyProp2 { get; set; }

        [JsonPropertyOrder(1)]
        public string MyProp3 { get; set; }
    }

    [Fact]
    public void PropertyOrder()
    {
        var model = new TestModel2()
        {
            MyProp = nameof(TestModel2.MyProp),
            MyProp2 = nameof(TestModel2.MyProp2),
            MyProp3 = nameof(TestModel2.MyProp3),
        };

        var yaml = YamlConverter.Serialize(model);

        string expected = """
                          MyProp3: MyProp3
                          MyProp2: MyProp2
                          MyProp: MyProp

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void DisablePropertyOrder()
    {
        var model = new TestModel2()
        {
            MyProp = nameof(TestModel2.MyProp),
            MyProp2 = nameof(TestModel2.MyProp2),
            MyProp3 = nameof(TestModel2.MyProp3),
        };

        var yaml = YamlConverter.Serialize(model, ignoreOrder: true);

        string expected = """
                          MyProp: MyProp
                          MyProp2: MyProp2
                          MyProp3: MyProp3

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }
}
