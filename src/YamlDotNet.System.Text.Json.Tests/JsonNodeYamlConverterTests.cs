using System.Text.Json.Nodes;

namespace YamlDotNet.System.Text.Json.Tests;

public class JsonNodeYamlConverterTests
{
    [Theory]
    [InlineData("25")]
    [InlineData("\"25\"")]
    [InlineData("1.55")]
    [InlineData("\"1.55\"")]
    [InlineData("\"my string\"")]
    [InlineData("\"test\\ntest2\\ntest3\"")]
    [InlineData("true")]
    [InlineData("false")]
    //[InlineData("null")]
    //[InlineData("{}")]
    [InlineData("\"\"")]
    public void JsonValueTests(string val)
    {
        var input = JsonValue.Parse(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonValue>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory]
    [InlineData("{\"Temperature\": \"25\"}")]
    [InlineData("{\"Temperature\": 25}")]
    [InlineData("{\"Temperature\": \"test\\ntest2\\ntest3\"}")]
    [InlineData("{\"Temperatures\": [\"1\",\"2\",\"3\"]}")]
    [InlineData("{\"Temperatures\": [1,2,3]}")]
    [InlineData("{\"Temperature\": {\"City\": \"Vancouver\",\"Temp\": 25}}")]
    [InlineData("{\"Temperature\": null}")]
    [InlineData("{\"Temperature\": \"\"}")]
    [InlineData("{\"Temperatures\": [{\"Prop\": 1},{\"Prop\": 2},{\"Prop\": 3}]}")]
    [InlineData("{\"Temperatures\": [[{\"Prop\": 1},{\"Prop\": 11},{\"Prop\": 111}],[{\"Prop\": 2},{\"Prop\": 22},{\"Prop\": 222}],[{\"Prop\": 3},{\"Prop\": 33},{\"Prop\": 333}]]}")]
    [InlineData("25")]
    [InlineData("\"25\"")]
    [InlineData("1.55")]
    [InlineData("\"1.55\"")]
    [InlineData("\"my string\"")]
    [InlineData("\"test\\ntest2\\ntest3\"")]
    [InlineData("true")]
    [InlineData("false")]
    public void JsonNodeTests(string val)
    {
        var input = JsonNode.Parse(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonNode>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory]
    [InlineData("{\"Temperature\": \"25\"}")]
    [InlineData("{\"Temperature\": 25}")]
    [InlineData("{\"Temperatures\": [\"1\",\"2\",\"3\"]}")]
    [InlineData("{\"Temperatures\": [1,2,3]}")]
    [InlineData("{\"Temperature\": {\"City\": \"Vancouver\",\"Temp\": 25}}")]
    [InlineData("{\"Temperature\": null}")]
    [InlineData("{\"Temperature\": \"\"}")]
    [InlineData("{\"Temperatures\": [{\"Prop\": 1},{\"Prop\": 2},{\"Prop\": 3}]}")]
    [InlineData("{\"Temperatures\": [[{\"Prop\": 1},{\"Prop\": 11},{\"Prop\": 111}],[{\"Prop\": 2},{\"Prop\": 22},{\"Prop\": 222}],[{\"Prop\": 3},{\"Prop\": 33},{\"Prop\": 333}]]}")]
    public void JsonObjectTests(string val)
    {
        var input = JsonNode.Parse(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonObject>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory]
    [InlineData("[\"1\",\"2\",\"3\"]")]
    [InlineData("[1,2,3]")]
    [InlineData("[{\"Temperature\": \"11\"},{\"Temperature\": \"22\"}]")]
    [InlineData("[1,2,null]")]
    [InlineData("[{\"Prop\": {\"Prop\": 1}},{\"Prop\": {\"Prop\": 2}},{\"Prop\": {\"Prop\": 3}}]")]
    [InlineData("[[{\"Prop\": 1},{\"Prop\": 11},{\"Prop\": 111}],[{\"Prop\": 2},{\"Prop\": 22},{\"Prop\": 222}],[{\"Prop\": 3},{\"Prop\": 33},{\"Prop\": 333}]]")]
    [InlineData("[]")]
    public void JsonArrayTests(string val)
    {
        var input = JsonNode.Parse(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonArray>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }
}