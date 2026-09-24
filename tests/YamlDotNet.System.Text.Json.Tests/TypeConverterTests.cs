using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YamlDotNet.Core;
using TestFixtures;

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
        var input = JsonNode.Parse(val)!.AsValue();

        var yaml = StaticYaml.Serialize(input!);

        var output = StaticYaml.Deserialize<JsonValue>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetArrayTests))]
    public void JsonArrayTests(string val)
    {
        var input = JsonNode.Parse(val)!.AsArray();

        var yaml = StaticYaml.Serialize(input!);

        var output = StaticYaml.Deserialize<JsonArray>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetObjectTests))]
    public void JsonObjectTests(string val)
    {
        var input = JsonNode.Parse(val)!.AsObject();

        var yaml = StaticYaml.Serialize(input!);

        var output = StaticYaml.Deserialize<JsonObject>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonNodeTests(string val)
    {
        var input = JsonNode.Parse(val);

        var yaml = StaticYaml.Serialize(input!);

        var output = StaticYaml.Deserialize<JsonNode>(yaml);

        Assert.Equal(val, output.ToJsonString(JsonSerializerOptions));
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonElementTests(string val)
    {
        var input = SharedJson.ParseElement(val);

        var yaml = StaticYaml.Serialize(input);

        var output = StaticYaml.Deserialize<JsonElement>(yaml);

        Assert.True(JsonElement.DeepEquals(input, output));
    }

    [Theory]
    [MemberData(nameof(GetValueTests))]
    [MemberData(nameof(GetObjectTests))]
    [MemberData(nameof(GetArrayTests))]
    public void JsonDocumentTests(string val)
    {
        var input = JsonDocument.Parse(val);

        var yaml = StaticYaml.Serialize(input!);

        var output = StaticYaml.Deserialize<JsonDocument>(yaml);

        Assert.True(JsonElement.DeepEquals(input.RootElement, output.RootElement));
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
        var input = JsonNode.Parse(inputVal);

        var yaml = StaticYaml.Serialize(input!, sortAlphabetically: true);

        var output = StaticYaml.Deserialize<JsonNode>(yaml);

        Assert.Equal(outputVal, output.ToJsonString(JsonSerializerOptions));
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

        var output = StaticYaml.Deserialize<FixtureModels.UnmatchedProperties.V1CustomResourceDefinition>(yaml, true);

        var yamlOutput = StaticYaml.Serialize(output);

        var yamlExpected = """
                    apiVersion: 1.2.3
                    kind: CustomResourceDefinition
                    metadata:
                      name: Test
                    
                    """;

        Assert.Equal(yamlExpected.ReplaceLineEndings(), yamlOutput.ReplaceLineEndings());
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

        Assert.Throws<YamlException>(() => StaticYaml.Deserialize<FixtureModels.UnmatchedProperties.V1CustomResourceDefinition>(yaml));
    }
}
