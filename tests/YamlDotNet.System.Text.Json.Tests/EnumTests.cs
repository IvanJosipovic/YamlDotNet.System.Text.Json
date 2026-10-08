using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class EnumTests
{
    [Fact]
    public void EnumSerialize()
    {
        var model = new FixtureModels.Enums.EnumModel()
        {
            Enum = FixtureModels.Enums.TestEnum.Third,
            EnumName = FixtureModels.Enums.TestEnum.Second,
            EnumList =
            [
                FixtureModels.Enums.TestEnum.Second
            ]
        };

        var yaml = StaticYaml.Serialize(model);

        var expected = """
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

        var model = StaticYaml.Deserialize<FixtureModels.Enums.EnumModel>(yaml);

        model.Enum.ShouldBe(FixtureModels.Enums.TestEnum.Third);
        model.EnumName.ShouldBe(FixtureModels.Enums.TestEnum.Second);
        model.EnumNull.ShouldBeNull();
        model.EnumList!.Count.ShouldBe(1);
        model.EnumList[0].ShouldBe(FixtureModels.Enums.TestEnum.Second);
    }
}
