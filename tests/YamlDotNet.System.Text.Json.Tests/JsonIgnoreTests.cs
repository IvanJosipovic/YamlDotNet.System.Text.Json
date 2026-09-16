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

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? HideWhenNull { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ShowWhenNotNull { get; set; } = nameof(ShowWhenNotNull);

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int HideWhenDefault { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int ShowWhenNotDefault { get; set; } = 42;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<int> ShowWhenEmpty { get; set; } = [];

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int? HideNullableWhenDefault { get; set; } = 0;
    }

    public class DirectionalTestModel
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public string WriteOnly { get; set; } = nameof(WriteOnly);

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
        public string ReadOnly { get; set; } = nameof(ReadOnly);

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
        public string IncludedWriteOnlyField = nameof(IncludedWriteOnlyField);

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
        public string IncludedReadOnlyField = nameof(IncludedReadOnlyField);

        [JsonInclude]
        [JsonPropertyName("aliasedField")]
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string IgnoredAliasedField = nameof(IgnoredAliasedField);

        [JsonInclude]
        [JsonPropertyName("extensionKey")]
        public string ExtensionKey = nameof(ExtensionKey);

        [JsonInclude]
        [JsonExtensionData]
        public Dictionary<string, object> ExtensionData = [];
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
                          ShowWhenNotNull: ShowWhenNotNull
                          ShowWhenNotDefault: 42
                          ShowWhenEmpty: []

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

    [Fact]
    public void Serialize_RespectsConditionalIgnoreConditions()
    {
        var model = new TestModel();

        var yaml = YamlConverter.Serialize(model);

        yaml.ShouldNotContain("HideWhenNull");
        yaml.ShouldContain("ShowWhenNotNull: ShowWhenNotNull");
        yaml.ShouldNotContain("HideWhenDefault");
        yaml.ShouldContain("ShowWhenNotDefault: 42");
        yaml.ShouldContain("ShowWhenEmpty: []");
        yaml.ShouldNotContain("HideNullableWhenDefault");
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

        var model = YamlConverter.Deserialize<TestModel>(yaml);

        model.HideWhenNull.ShouldBe("supplied");
        model.ShowWhenNotNull.ShouldBe("supplied2");
        model.HideWhenDefault.ShouldBe(7);
        model.ShowWhenNotDefault.ShouldBe(8);
        model.ShowWhenEmpty.ShouldBe([1]);
        model.Hide.ShouldBe(nameof(TestModel.Hide));
        model.Hide2.ShouldBe(nameof(TestModel.Hide2));
    }

    [Fact]
    public void Serialize_RespectsDirectionalIgnoreConditions()
    {
        var yaml = YamlConverter.Serialize(new DirectionalTestModel());

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

        var model = YamlConverter.Deserialize<DirectionalTestModel>(yaml);

        model.WriteOnly.ShouldBe("supplied");
        model.ReadOnly.ShouldBe(nameof(DirectionalTestModel.ReadOnly));
        model.IncludedWriteOnlyField.ShouldBe("supplied-field");
        model.IncludedReadOnlyField.ShouldBe(nameof(DirectionalTestModel.IncludedReadOnlyField));
        model.IgnoredAliasedField.ShouldBe(nameof(DirectionalTestModel.IgnoredAliasedField));
        model.ExtensionKey.ShouldBe("extension-value");
        model.ExtensionData.ShouldContainKeyAndValue("extraKey", "extra-value");
    }
}
