
namespace YamlDotNet.System.Text.Json.Tests;

public class Json2YamlTests
{
    [Fact]
    public void Test1()
    {
        var json = """
                   {
                    "Temperature": "25"
                   }
                   """;

        var yaml = YamlConverter.SerializeJson(json);

        var expected = """
                          Temperature: '25'

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

    [Fact]
    public void Test2()
    {
        var json = """
                   {
                    "b": "2",
                    "a": "1"
                   }
                   """;

        var yaml = YamlConverter.SerializeJson(json, sortAlphabetically: true);

        var expected = """
                          a: '1'
                          b: '2'

                          """;
        yaml.ReplaceLineEndings().ShouldBe(expected.ReplaceLineEndings());
    }

}
