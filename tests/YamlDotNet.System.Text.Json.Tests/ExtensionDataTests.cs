using System.Text.Json;
using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class ExtensionDataTests
{
    [Fact]
    public void SerializeExtensionData()
    {
        var model = new FixtureModels.ExtensionData.JsonElementModel()
        {
            ExtensionData = new()
            {
                { "test", SharedJson.ParseElement("\"test-value\"") }
            }
        };

        var yaml = StaticYaml.Serialize(model);

        var expected = """
                       test: test-value

                       """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void SerializeExtensionDataObject()
    {
        var model = new FixtureModels.ExtensionData.ObjectModel()
        {
            ExtensionData = new()
            {
                { "test", "test-value" }
            }
        };

        var yaml = StaticYaml.Serialize(model);

        var expected = """
                       test: test-value

                       """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void SerializeExtensionDataMixed()
    {
        var model = new FixtureModels.ExtensionData.MixedModel()
        {
            before = "test1",

            ExtensionData = new()
            {
                { "test", SharedJson.ParseElement("\"test-value\"") }
            },

            after = "test2"
        };

        var yaml = StaticYaml.Serialize(model);

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

        var model = StaticYaml.Deserialize<FixtureModels.ExtensionData.JsonElementModel>(yaml);

        model.ExtensionData!.Count.ShouldBe(1);
        model.ExtensionData["test"].GetString().ShouldBe("test-value");
    }

    [Fact]
    public void DeserializeNestedExtensionData()
    {
        var yaml = """
                   nested:
                     test: test-value
                     values:
                       - one
                       - two

                   """;

        var model = StaticYaml.Deserialize<FixtureModels.ExtensionData.JsonElementModel>(yaml);
        var nested = model.ExtensionData!["nested"];

        nested.GetProperty("test").GetString().ShouldBe("test-value");
        nested.GetProperty("values")[1].GetString().ShouldBe("two");
    }

    [Fact]
    public void DeserializeExtensionDataObject()
    {
        var yaml = """
                   test: test-value

                   """;

        var model = StaticYaml.Deserialize<FixtureModels.ExtensionData.ObjectModel>(yaml);

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

        var model = StaticYaml.Deserialize<FixtureModels.ExtensionData.MixedModel>(yaml);

        model.before.ShouldBe("test1");
        model.after.ShouldBe("test2");

        model.ExtensionData!.Count.ShouldBe(1);
        model.ExtensionData["test"].GetString().ShouldBe("test-value");
    }
}
