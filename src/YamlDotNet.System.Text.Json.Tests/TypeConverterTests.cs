using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YamlDotNet.Core;

namespace YamlDotNet.System.Text.Json.Tests;

public class TypeConverterTests
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static IEnumerable<object[]> GetValueTests()
    {
        return new List<object[]>
        {
            new object[] { "25" },
            new object[] { "\"25\"" },
            new object[] { "1.55" },
            new object[] { "1E+300" },
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
            new object[] { "{\"Temperature\":\"25\"}" },
            new object[] { "{\"Temperature\":25}" },
            new object[] { "{\"Temperature\":\"test\\ntest2\\ntest3\"}" },
            new object[] { "{\"Temperatures\":[\"1\",\"2\",\"3\"]}" },
            new object[] { "{\"Temperatures\":[1,2,3]}" },
            new object[] { "{\"Temperature\":{\"City\":\"Vancouver\",\"Temp\":25}}" },
            new object[] { "{\"Temperature\":null}" },
            new object[] { "{\"Temperature\":\"\"}" },
            new object[] { "{\"Temperature\":[]}" },
            new object[] { "{\"Temperature\":{}}" },
            new object[] { "{\"Temperatures\":[{\"Prop\":1},{\"Prop\":2},{\"Prop\":3}]}" },
            new object[] { "{\"Temperatures\":[[{\"Prop\":1},{\"Prop\":11},{\"Prop\":111}],[{\"Prop\":2},{\"Prop\":22},{\"Prop\":222}],[{\"Prop\":3},{\"Prop\":33},{\"Prop\":333}]]}" },
            new object[] { "{\"url\":\"{\\\"config\\\":{\\\"entries\\\":[{\\\"url\\\":\\\"http://service.svc.cluster.local:7002/policy-data\\\",\\\"topics\\\":[\\\"policy_data\\\"]}]}}\"}" },
            new object[] { "{\"KEY1\":{\"NAME\":\"XXXXXX\",\"VALUE\":100},\"KEY2\":{\"NAME\":\"YYYYYYY\",\"VALUE\":200},\"KEY3\":{\"NAME\":\"ZZZZZZZ\",\"VALUE\":500}}" },
        };
    }

    public static IEnumerable<object[]> GetArrayTests()
    {
        return new List<object[]>
        {
            new object[] { "[\"1\",\"2\",\"3\"]" },
            new object[] { "[1,2,3]" },
            new object[] { "[{\"Temperature\":\"11\"},{\"Temperature\":\"22\"}]" },
            new object[] { "[1,2,null]" },
            new object[] { "[{\"Prop\":{\"Prop\":1}},{\"Prop\":{\"Prop\":2}},{\"Prop\":{\"Prop\":3}}]" },
            new object[] { "[[{\"Prop\":1},{\"Prop\":11},{\"Prop\":111}],[{\"Prop\":2},{\"Prop\":22},{\"Prop\":222}],[{\"Prop\":3},{\"Prop\":33},{\"Prop\":333}]]" },
            new object[] { "[]" },
            new object[] { "[{\"KEY1\":{\"NAME\":\"XXXXXX\",\"VALUE\":100},\"KEY2\":{\"NAME\":\"YYYYYYY\",\"VALUE\":200},\"KEY3\":{\"NAME\":\"ZZZZZZZ\",\"VALUE\":500}}]" },
            new object[] { "[true,false]" },
            new object[] { "[\"true\",\"false\"]" },
            new object[] { "[{},{}]" },
            new object[] { "[1,2,{}]" },
        };
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    public void JsonValueTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonValue>(val, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input!);

        var output = YamlConverter.Deserialize<JsonValue>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetArrayTests))]
    public void JsonArrayTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonArray>(val, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input!);

        var output = YamlConverter.Deserialize<JsonArray>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetObjectTests))]
    public void JsonObjectTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonObject>(val, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input!);

        var output = YamlConverter.Deserialize<JsonObject>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonNodeTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonNode>(val, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input!);

        var output = YamlConverter.Deserialize<JsonNode>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonElementTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonElement>(val, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input);

        var output = YamlConverter.Deserialize<JsonElement>(yaml);

        Assert.Equal(val, JsonSerializer.Serialize(output, JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonDocumentTests(string val)
    {
        var input = JsonSerializer.Deserialize<JsonDocument>(val, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input!);

        var output = YamlConverter.Deserialize<JsonDocument>(yaml);

        Assert.Equal(val, JsonSerializer.Serialize(output, JsonSerializerOptions));
    }

    public static IEnumerable<object[]> GetObjectSortTests()
    {
        return new List<object[]>
        {
            new object[] { "{\"b\":\"2\",\"a\":\"1\"}", "{\"a\":\"1\",\"b\":\"2\"}" },
            new object[] { "{\"b\":\"2\",\"a\":\"1\",\"c\":\"3\"}", "{\"a\":\"1\",\"b\":\"2\",\"c\":\"3\"}" },
            new object[] { "{\"nested\":{\"b\":2,\"c\":3,\"a\":1}}", "{\"nested\":{\"a\":1,\"b\":2,\"c\":3}}" }
        };
    }

    [Theory]
    [MemberData(nameof(GetObjectSortTests))]
    public void JsonNodeSortTests(string inputVal, string outputVal)
    {
        var input = JsonSerializer.Deserialize<JsonNode>(inputVal, JsonSerializerOptions);

        var yaml = YamlConverter.Serialize(input!, sortAlphabetically: true);

        var output = YamlConverter.Deserialize<JsonNode>(yaml);

        Assert.Equal(outputVal, output.ToJsonString(JsonSerializerOptions));
    }

    public class V1ObjectMeta
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = default!;
    }

    public class V1CustomResourceDefinition
    {
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = default!;

        [JsonPropertyName("kind")]
        public string Kind { get; set; } = default!;

        [JsonPropertyName("metadata")]
        public V1ObjectMeta Metadata { get; set; } = default!;
    }

    [Fact]
    public void DeserializeUnmatched()
    {
        var yaml = """
                    apiVersion: 1.2.3
                    kind: CustomResourceDefinition
                    metadata:
                      name: Test
                    annotations:
                      test: value
                    """;

        var output = YamlConverter.Deserialize<V1CustomResourceDefinition>(yaml, true);

        var yamlOutput = YamlConverter.Serialize(output);

        var yamlExpected = """
                    apiVersion: 1.2.3
                    kind: CustomResourceDefinition
                    metadata:
                      name: Test
                    
                    """;

        Assert.Equal(yamlExpected, yamlOutput);
    }

    [Fact]
    public void DeserializeUnmatchedException()
    {
        var yaml = """
                    apiVersion: 1.2.3
                    kind: CustomResourceDefinition
                    metadata:
                      name: Test
                    annotations:
                      test: value
                    """;

        Assert.Throws<YamlException>(() => YamlConverter.Deserialize<V1CustomResourceDefinition>(yaml));
    }
}
