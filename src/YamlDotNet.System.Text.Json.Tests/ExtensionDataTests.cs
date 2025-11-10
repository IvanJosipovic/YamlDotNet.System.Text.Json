using System.Text.Json;
using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class ExtensionDataTests
{
    public class TestJsonExtensionDataModel
    {
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }

    public class TestJsonExtensionDataModelObject
    {
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }
    }

    public class TestJsonExtensionDataModelMixed
    {
        public string? before { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }

        public string? after { get; set; }
    }

    [Fact]
    public void SerializeExtensionData()
    {
        var model = new TestJsonExtensionDataModel()
        {
            ExtensionData = new()
            {
                { "test", JsonSerializer.SerializeToElement("test-value") }
            }
        };

        var yaml = YamlConverter.Serialize(model);

        var expected = """
                       test: test-value

                       """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void SerializeExtensionDataObject()
    {
        var model = new TestJsonExtensionDataModelObject()
        {
            ExtensionData = new()
            {
                { "test", "test-value" }
            }
        };

        var yaml = YamlConverter.Serialize(model);

        var expected = """
                       test: test-value

                       """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void SerializeExtensionDataMixed()
    {
        var model = new TestJsonExtensionDataModelMixed()
        {
            before = "test1",

            ExtensionData = new()
            {
                { "test", JsonSerializer.SerializeToElement("test-value") }
            },

            after = "test2"
        };

        var yaml = YamlConverter.Serialize(model);

        var expected = """
                       before: test1
                       test: test-value
                       after: test2

                       """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void DeserializeExtensionData()
    {
        var yaml = """
                   test: test-value

                   """;

        var model = YamlConverter.Deserialize<TestJsonExtensionDataModel>(yaml);

        model.ExtensionData!.Count.ShouldBe(1);
        model.ExtensionData["test"].GetString().ShouldBe("test-value");
    }

    [Fact]
    public void DeserializeExtensionDataObject()
    {
        var yaml = """
                   test: test-value

                   """;

        var model = YamlConverter.Deserialize<TestJsonExtensionDataModelObject>(yaml);

        model.ExtensionData!.Count.ShouldBe(1);
        model.ExtensionData["test"].ShouldBe("test-value");
    }

    [Fact]
    public void DeserializeExtensionDataMixed()
    {
        var yaml = """
                   before: test1
                   test: test-value
                   after: test2

                   """;

        var model = YamlConverter.Deserialize<TestJsonExtensionDataModelMixed>(yaml);

        model.before.ShouldBe("test1");
        model.after.ShouldBe("test2");

        model.ExtensionData!.Count.ShouldBe(1);
        model.ExtensionData["test"].GetString().ShouldBe("test-value");
    }
}
