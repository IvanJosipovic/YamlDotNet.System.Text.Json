using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class JsonIgnoreTests
{
    [Fact]
    public void Serialize()
    {
        var model = new FixtureModels.IgnoreConditions.ConditionsModel();

        var yaml = StaticYaml.Serialize(model);

        var expected = """
                          MyProp: MyProp
                          Show: Show
                          Show2: Show2
                          Show3: Show3
                          ShowWhenNotNull: ShowWhenNotNull
                          ShowWhenNotDefault: 42
                          ShowWhenEmpty: []
                          HideNullableWhenDefault: 0

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

        var model = StaticYaml.Deserialize<FixtureModels.IgnoreConditions.ConditionsModel>(yaml);

        model.MyProp.ShouldBe("test");
        model.Show.ShouldBe("test5");
        model.Show2.ShouldBe("test6");
        model.Show3.ShouldBe("test7");
    }

    [Fact]
    public void Serialize_RespectsConditionalIgnoreConditions()
    {
        var model = new FixtureModels.IgnoreConditions.ConditionsModel();

        var yaml = StaticYaml.Serialize(model);

        yaml.ShouldNotContain("HideWhenNull");
        yaml.ShouldContain("ShowWhenNotNull: ShowWhenNotNull");
        yaml.ShouldNotContain("HideWhenDefault");
        yaml.ShouldContain("ShowWhenNotDefault: 42");
        yaml.ShouldContain("ShowWhenEmpty: []");
        yaml.ShouldContain("HideNullableWhenDefault: 0");
    }

    [Fact]
    public void Deserialize_ConditionalIgnoreConditionsStillReadValues()
    {
        var yaml = """
                    HideWhenNull: supplied
                    ShowWhenNotNull: supplied2
                    HideWhenDefault: 7
                    ShowWhenNotDefault: 8
                    ShowWhenEmpty:
                      - 1
                    Hide: supplied3
                    Hide2: supplied4

                    """;

        var model = StaticYaml.Deserialize<FixtureModels.IgnoreConditions.ConditionsModel>(yaml);

        model.HideWhenNull.ShouldBe("supplied");
        model.ShowWhenNotNull.ShouldBe("supplied2");
        model.HideWhenDefault.ShouldBe(7);
        model.ShowWhenNotDefault.ShouldBe(8);
        model.ShowWhenEmpty.ShouldBe([1]);
        model.Hide.ShouldBe(nameof(FixtureModels.IgnoreConditions.ConditionsModel.Hide));
        model.Hide2.ShouldBe(nameof(FixtureModels.IgnoreConditions.ConditionsModel.Hide2));
    }

    [Fact]
    public void Serialize_RespectsDirectionalIgnoreConditions()
    {
        var yaml = StaticYaml.Serialize(new FixtureModels.IgnoreConditions.DirectionalModel());

        yaml.ShouldNotContain("WriteOnly");
        yaml.ShouldContain("ReadOnly: ReadOnly");
        yaml.ShouldNotContain("IncludedWriteOnlyField");
        yaml.ShouldContain("IncludedReadOnlyField: IncludedReadOnlyField");
        yaml.ShouldNotContain("aliasedField");
    }

    [Fact]
    public void Deserialize_RespectsDirectionalIgnoreConditions()
    {
        var yaml = """
                    WriteOnly: supplied
                    ReadOnly: supplied
                    IncludedWriteOnlyField: supplied-field
                    IncludedReadOnlyField: ignored-field
                    aliasedField: ignored-alias
                    extensionKey: extension-value
                    extraKey: extra-value

                    """;

        var model = StaticYaml.Deserialize<FixtureModels.IgnoreConditions.DirectionalModel>(yaml);

        model.WriteOnly.ShouldBe("supplied");
        model.ReadOnly.ShouldBe(nameof(FixtureModels.IgnoreConditions.DirectionalModel.ReadOnly));
        model.IncludedWriteOnlyField.ShouldBe("supplied-field");
        model.IncludedReadOnlyField.ShouldBe(nameof(FixtureModels.IgnoreConditions.DirectionalModel.IncludedReadOnlyField));
        model.IgnoredAliasedField.ShouldBe(nameof(FixtureModels.IgnoreConditions.DirectionalModel.IgnoredAliasedField));
        model.ExtensionKey.ShouldBe("extension-value");
        model.ExtensionData.ShouldContainKeyAndValue("extraKey", "extra-value");
    }
}
