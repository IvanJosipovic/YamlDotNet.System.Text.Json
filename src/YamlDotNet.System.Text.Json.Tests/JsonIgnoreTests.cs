using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class JsonIgnoreTests
{
    public class TestModel
    {
        public string MyProp { get; set; } = nameof(TestModel.MyProp);

        [JsonIgnore()]
        public string Hide { get; set; } = nameof(TestModel.Hide);

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string Hide2 { get; set; } = nameof(TestModel.Hide2);

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Show { get; set; } = nameof(TestModel.Show);

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Show2 { get; set; } = nameof(TestModel.Show2);

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Show3 { get; set; } = nameof(TestModel.Show3);
    }

    [Fact]
    public void Serialize()
    {
        var model = new TestModel();

        var yaml = YamlConverter.Serialize(model);

        var expected = """
                          MyProp: MyProp
                          Show: Show
                          Show2: Show2
                          Show3: Show3

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void Deserialize()
    {
        var yaml = """
                    MyProp: test
                    Show: test5
                    Show2: test6
                    Show3: test7

                    """;

        var model = YamlConverter.Deserialize<TestModel>(yaml);

        model.MyProp.ShouldBe("test");
        model.Show.ShouldBe("test5");
        model.Show2.ShouldBe("test6");
        model.Show3.ShouldBe("test7");
    }
}
