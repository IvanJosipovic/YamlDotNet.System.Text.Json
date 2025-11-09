using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class EnumTests
{
    public class TestModel
    {
        public TestEnum? Enum { get; set; }

        public TestEnum? EnumName { get; set; }

        public TestEnum? EnumNull { get; set; }

        public IList<TestEnum>? EnumList { get; set; }
    }

    public enum TestEnum
    {
        First,
        [JsonStringEnumMemberName("val2")]
        Second,
        Third
    }


    [Fact]
    public void EnumSerialize()
    {
        var model = new TestModel()
        {
            Enum = TestEnum.Third,
            EnumName = TestEnum.Second,
            EnumList =
            [
                TestEnum.Second
            ]
        };

        var yaml = YamlConverter.Serialize(model);

        string expected = """
                          Enum: Third
                          EnumName: val2
                          EnumNull: 
                          EnumList:
                          - val2

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void EnumNameDeserialize()
    {
        var yaml = """
                    Enum: Third
                    EnumName: val2
                    EnumNull: 
                    EnumList:
                    - val2

                    """;

        var model = YamlConverter.Deserialize<TestModel>(yaml);

        model.Enum.ShouldBe(TestEnum.Third);
        model.EnumName.ShouldBe(TestEnum.Second);
        model.EnumNull.ShouldBeNull();
        model.EnumList.Count.ShouldBe(1);
        model.EnumList[0].ShouldBe(TestEnum.Second);
    }
}