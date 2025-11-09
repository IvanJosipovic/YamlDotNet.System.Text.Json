using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class JsonPropertyTypeInspectorTests
{
    public class TestModel
    {
        [JsonPropertyName("MyNewPropName")]
        public string MyProp { get; set; }

        public string MyProp2 { get; set; }

        [JsonIgnore()]
        public string Hide { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string Hide2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Show { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Show2 { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Show3 { get; set; }
    }

    [Fact]
    public void Serialize()
    {
        var model = new TestModel()
        {
            MyProp = nameof(TestModel.MyProp),
            MyProp2 = nameof(TestModel.MyProp2),
            Hide = nameof(TestModel.Hide),
            Hide2 = nameof(TestModel.Hide2),
            Show = nameof(TestModel.Show),
            Show2 = nameof(TestModel.Show2),
            Show3 = nameof(TestModel.Show3),
        };

        var yaml = YamlConverter.Serialize(model);

        string expected = """
                          MyNewPropName: MyProp
                          MyProp2: MyProp2
                          Show: Show
                          Show2: Show2
                          Show3: Show3

                          """;
        yaml.ShouldBe(expected);
    }

    [Fact]
    public void Deserialize()
    {
        var yaml = """
                    MyNewPropName: test
                    MyProp2: test2
                    Show: test5
                    Show2: test6
                    Show3: test7

                    """;

        var model = YamlConverter.Deserialize<TestModel>(yaml);

        model.MyProp.ShouldBe("test");
        model.MyProp2.ShouldBe("test2");
        model.Show.ShouldBe("test5");
        model.Show2.ShouldBe("test6");
        model.Show3.ShouldBe("test7");
    }
}
