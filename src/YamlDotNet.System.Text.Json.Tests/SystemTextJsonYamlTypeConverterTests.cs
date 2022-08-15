using System.Text.Json;
using System.Text.Json.Nodes;

namespace YamlDotNet.System.Text.Json.Tests;

public class SystemTextJsonYamlTypeConverterTests
{
    public static IEnumerable<object[]> GetValueTests()
    {
        return new List<object[]>
        {
            new object[] { "25" },
            new object[] { "\"25\"" },
            new object[] { "1.55" },
            new object[] { "\"my string\"" },
            new object[] { "\"my string\"" },
            new object[] { "\"test\\ntest2\\ntest3\"" },
            new object[] { "true" },
            new object[] { "false" },
            new object[] { "\"\"" },
            //new object[] { "null" }
        };
    }

    public static IEnumerable<object[]> GetObjectTests()
    {
        return new List<object[]>
        {
            new object[] { "{\"Temperature\": \"25\"}" },
            new object[] { "{\"Temperature\": 25}" },
            new object[] { "{\"Temperature\": \"test\\ntest2\\ntest3\"}" },
            new object[] { "{\"Temperatures\": [\"1\",\"2\",\"3\"]}" },
            new object[] { "{\"Temperatures\": [1,2,3]}" },
            new object[] { "{\"Temperature\": {\"City\": \"Vancouver\",\"Temp\": 25}}" },
            new object[] { "{\"Temperature\": null}" },
            new object[] { "{\"Temperature\": \"\"}" },
            new object[] { "{\"Temperature\": []}" },
            new object[] { "{\"Temperature\": {}}" },
            new object[] { "{\"Temperatures\": [{\"Prop\": 1},{\"Prop\": 2},{\"Prop\": 3}]}" },
            new object[] { "{\"Temperatures\": [[{\"Prop\": 1},{\"Prop\": 11},{\"Prop\": 111}],[{\"Prop\": 2},{\"Prop\": 22},{\"Prop\": 222}],[{\"Prop\": 3},{\"Prop\": 33},{\"Prop\": 333}]]}" },
            new object[] { "{ \"url\": \"{\\\"config\\\":{\\\"entries\\\":[{\\\"url\\\":\\\"http://service.svc.cluster.local:7002/policy-data\\\",\\\"topics\\\":[\\\"policy_data\\\"]}]}}\"}" },
            new object[] { "{\"KEY1\":{\"NAME\":\"XXXXXX\",\"VALUE\":100},\"KEY2\":{\"NAME\":\"YYYYYYY\",\"VALUE\":200},\"KEY3\":{\"NAME\":\"ZZZZZZZ\",\"VALUE\":500}}" },
        };
    }

    public static IEnumerable<object[]> GetArrayTests()
    {
        return new List<object[]>
        {
            new object[] { "[\"1\",\"2\",\"3\"]" },
            new object[] { "[1,2,3]" },
            new object[] { "[{\"Temperature\": \"11\"},{\"Temperature\": \"22\"}]" },
            new object[] { "[1,2,null]" },
            new object[] { "[{\"Prop\": {\"Prop\": 1}},{\"Prop\": {\"Prop\": 2}},{\"Prop\": {\"Prop\": 3}}]" },
            new object[] { "[[{\"Prop\": 1},{\"Prop\": 11},{\"Prop\": 111}],[{\"Prop\": 2},{\"Prop\": 22},{\"Prop\": 222}],[{\"Prop\": 3},{\"Prop\": 33},{\"Prop\": 333}]]" },
            new object[] { "[]" },
            new object[] { "[{\"KEY1\":{\"NAME\":\"XXXXXX\",\"VALUE\":100},\"KEY2\":{\"NAME\":\"YYYYYYY\",\"VALUE\":200},\"KEY3\":{\"NAME\":\"ZZZZZZZ\",\"VALUE\":500}}]" },
            new object[] { "[true,false]" },
            new object[] { "[\"true\",\"false\"]" },
            new object[] { "[{},{}]" },
            new object[] { "[1,2,{}]" },
        };
    }

    [Theory(Timeout = 100)]
    [MemberData(nameof(GetValueTests))]
    public void JsonValueTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonValue>(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonValue>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory(Timeout = 100)]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    public void JsonNodeTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonNode>(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonNode>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory(Timeout = 100)]
    [MemberData(nameof(GetObjectTests))]
    public void JsonObjectTests(string val)
    {
        var input = JsonNode.Parse(val).AsObject();

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonObject>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory(Timeout = 100)]
    [MemberData(nameof(GetArrayTests))]
    public void JsonArrayTests(string val)
    {
        var input = JsonNode.Parse(val).AsArray();

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonArray>(yaml);

        Assert.Equal(input.ToJsonString(), output.ToJsonString());
    }

    [Theory(Timeout = 100)]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonElementTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonElement>(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonElement>(yaml);

        Assert.Equal(JsonSerializer.Serialize(input), JsonSerializer.Serialize(output));
    }

    [Theory(Timeout = 100)]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonDocumentTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonDocument>(val);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonDocument>(yaml);

        Assert.Equal(JsonSerializer.Serialize(input), JsonSerializer.Serialize(output));
    }
}