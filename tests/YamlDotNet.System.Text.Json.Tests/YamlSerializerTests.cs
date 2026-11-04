using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using YamlDotNet.Core;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class YamlSerializerTests
{
    public sealed class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    [JsonSerializable(typeof(Person))]
    [JsonSerializable(typeof(List<Person>))]
    [JsonSerializable(typeof(Person[]))]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    [JsonSerializable(typeof(Dictionary<string, int>))]
    [JsonSerializable(typeof(List<int>))]
    [JsonSerializable(typeof(List<string>))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(double))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(JsonElement))]
    internal sealed partial class AppJsonContext : JsonSerializerContext;

    private static JsonTypeInfo<T> TypeInfo<T>(Func<AppJsonContext, JsonTypeInfo<T>> selector)
    {
        return selector(AppJsonContext.Default);
    }

    [Fact]
    public void SerializeProducesYamlFromSourceGeneratedContext()
    {
        var person = new Person { Name = "John", Age = 30 };

        var yaml = YamlSerializer.Serialize(person, AppJsonContext.Default.Person);

        yaml.ShouldBe("Name: John" + Environment.NewLine + "Age: 30" + Environment.NewLine);
    }

    [Fact]
    public void DeserializeProducesObjectFromSourceGeneratedContext()
    {
        var person = YamlSerializer.Deserialize("""
            Name: Jane
            Age: 42
            """, AppJsonContext.Default.Person);

        person.ShouldNotBeNull();
        person.Name.ShouldBe("Jane");
        person.Age.ShouldBe(42);
    }

    [Fact]
    public void YamlAndJsonProduceEquivalentResults()
    {
        var person = new Person { Name = "John", Age = 30 };

        var json = JsonSerializer.Serialize(person, AppJsonContext.Default.Person);
        var yaml = YamlSerializer.Serialize(person, AppJsonContext.Default.Person);

        var fromJson = JsonSerializer.Deserialize(json, AppJsonContext.Default.Person);
        var fromYaml = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.Person);

        fromYaml.ShouldNotBeNull();
        fromYaml.Name.ShouldBe(fromJson!.Name);
        fromYaml.Age.ShouldBe(fromJson.Age);
    }

    [Fact]
    public void RoundTripsNestedCollectionsAndDictionaries()
    {
        var model = new Dictionary<string, string>
        {
            ["name"] = "api",
            ["version"] = "1.20",
        };

        var yaml = YamlSerializer.Serialize(model, AppJsonContext.Default.DictionaryStringString);
        var result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.DictionaryStringString);

        result.ShouldNotBeNull();
        result["name"].ShouldBe("api");
        result["version"].ShouldBe("1.20");
    }

    [Fact]
    public void RoundTripsNestedStructures()
    {
        var model = new List<Person>
        {
            new() { Name = "a", Age = 1 },
            new() { Name = "b", Age = 2 },
        };

        var yaml = YamlSerializer.Serialize(model, AppJsonContext.Default.ListPerson);
        var result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.ListPerson);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("a");
        result[1].Age.ShouldBe(2);
    }

    [Fact]
    public void RoundTripsQuotedAmbiguousStrings()
    {
        var model = new Dictionary<string, string>
        {
            ["enabled"] = "true",
            ["version"] = "1.20",
            ["count"] = "3",
            ["empty"] = "",
            ["nothing"] = "null",
            ["tilde"] = "~",
            ["date"] = "2026-01-01",
            ["hex"] = "0x1A",
        };

        var yaml = YamlSerializer.Serialize(model, AppJsonContext.Default.DictionaryStringString);
        var result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.DictionaryStringString);

        result.ShouldNotBeNull();

        foreach (var pair in model)
        {
            result[pair.Key].ShouldBe(pair.Value);
        }
    }

    [Fact]
    public void EmptyMappingsAndSequencesRoundTrip()
    {
        YamlSerializer.Deserialize("{}", AppJsonContext.Default.DictionaryStringString).ShouldNotBeNull().ShouldBeEmpty();
        YamlSerializer.Deserialize("[]", AppJsonContext.Default.ListInt32).ShouldNotBeNull().ShouldBeEmpty();
        YamlSerializer.Serialize(new Dictionary<string, string>(), AppJsonContext.Default.DictionaryStringString).ShouldBe("{}" + Environment.NewLine);
        YamlSerializer.Serialize(new List<int>(), AppJsonContext.Default.ListInt32).ShouldBe("[]" + Environment.NewLine);
    }

    [Fact]
    public void SupportsRootScalarsAndSequences()
    {
        YamlSerializer.Serialize(42, AppJsonContext.Default.Int32).ShouldBe("42" + Environment.NewLine);
        YamlSerializer.Deserialize("42", AppJsonContext.Default.Int32).ShouldBe(42);
        YamlSerializer.Serialize("hello", AppJsonContext.Default.String).ShouldBe("hello" + Environment.NewLine);
        YamlSerializer.Serialize(true, AppJsonContext.Default.Boolean).ShouldBe("true" + Environment.NewLine);
        YamlSerializer.Serialize(1.5, AppJsonContext.Default.Double).ShouldBe("1.5" + Environment.NewLine);
        YamlSerializer.Deserialize("- 1" + Environment.NewLine + "- 2" + Environment.NewLine, AppJsonContext.Default.ListInt32)
            .ShouldNotBeNull().ShouldBe(new List<int> { 1, 2 });
    }

    [Fact]
    public void ResolvesPlainScalarsPerJsonSemantics()
    {
        YamlScalarShouldBe("value: yes", JsonValueKind.String, "yes");
        YamlScalarShouldBe("value: on", JsonValueKind.String, "on");
        YamlScalarShouldBe("value: no", JsonValueKind.String, "no");
        YamlScalarShouldBe("value: off", JsonValueKind.String, "off");
        YamlScalarShouldBe("value: y", JsonValueKind.String, "y");
        YamlScalarShouldBe("value: 00123", JsonValueKind.Number, raw: "123");
        YamlScalarShouldBe("value: 1e3", JsonValueKind.Number, raw: "1000");
        YamlScalarShouldBe("value: 1.20", JsonValueKind.Number, raw: "1.20");
        YamlScalarShouldBe("value: 0.1", JsonValueKind.Number, raw: "0.1");
        YamlScalarShouldBe("value: -7", JsonValueKind.Number, raw: "-7");
        YamlScalarShouldBe("value: 2026-01-01", JsonValueKind.String, "2026-01-01");
        YamlScalarShouldBe("value: \"true\"", JsonValueKind.String, "true");
        YamlScalarShouldBe("value: \"00123\"", JsonValueKind.String, "00123");
        YamlScalarShouldBe("value: 0x1A", JsonValueKind.String, "0x1A");
        YamlScalarShouldBe("value: TRUE", JsonValueKind.True);
        YamlScalarShouldBe("value: False", JsonValueKind.False);
        YamlScalarShouldBe("value: ~", JsonValueKind.Null);
        YamlScalarShouldBe("value: NULL", JsonValueKind.Null);
        YamlScalarShouldBe("value:", JsonValueKind.Null);
        YamlScalarShouldBe("value: !!str 123", JsonValueKind.String, "123");
    }

    [Fact]
    public void RejectsSpecialFloatScalars()
    {
        foreach (var value in new[] { ".nan", ".inf", "-.inf" })
        {
            Should.Throw<YamlException>(() => YamlSerializer.Deserialize($"value: {value}", AppJsonContext.Default.JsonElement))
                .Message.ShouldContain("not supported");
        }
    }

    [Fact]
    public void SupportsBlockScalarsAndMultilineStrings()
    {
        var literal = YamlSerializer.Deserialize("value: |" + Environment.NewLine + "  line one" + Environment.NewLine + "  line two" + Environment.NewLine, AppJsonContext.Default.JsonElement);
        literal.GetProperty("value").ValueKind.ShouldBe(JsonValueKind.String);
        literal.GetProperty("value").GetString().ShouldBe("line one" + Environment.NewLine + "line two" + Environment.NewLine);

        var model = new Dictionary<string, string> { ["text"] = "alpha" + Environment.NewLine + "beta" };
        var yaml = YamlSerializer.Serialize(model, AppJsonContext.Default.DictionaryStringString);
        var result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.DictionaryStringString);

        result.ShouldNotBeNull();
        result["text"].ShouldBe("alpha" + Environment.NewLine + "beta");
    }

    [Fact]
    public void IgnoresComments()
    {
        var result = YamlSerializer.Deserialize("""
            # leading comment
            Name: John # trailing comment
            # another comment
            Age: 30
            """, AppJsonContext.Default.Person);

        result!.Name.ShouldBe("John");
        result.Age.ShouldBe(30);
    }

    [Fact]
    public void SupportsUnicode()
    {
        var model = new Dictionary<string, string> { ["emoji"] = "héllo 🎉" };

        var yaml = YamlSerializer.Serialize(model, AppJsonContext.Default.DictionaryStringString);
        var result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.DictionaryStringString);

        result.ShouldNotBeNull();
        result["emoji"].ShouldBe("héllo 🎉");
    }

    [Fact]
    public void SupportsEscapedStrings()
    {
        var model = new Dictionary<string, string> { ["text"] = "tab\tnewline\\quote\"end" };

        var yaml = YamlSerializer.Serialize(model, AppJsonContext.Default.DictionaryStringString);
        var result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.DictionaryStringString);

        result.ShouldNotBeNull();
        result["text"].ShouldBe(model["text"]);
    }

    [Fact]
    public void RejectsAliases()
    {
        var exception = Should.Throw<YamlException>(() => YamlSerializer.Deserialize("""
            base: &anchor
              Name: John
            copy: *anchor
            """, AppJsonContext.Default.Person));

        exception.Message.ShouldContain("anchors and aliases");
    }

    [Fact]
    public void RejectsMergeKeys()
    {
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("""
            anchor: &base
              Name: John
            merged:
              <<: *base
            """, AppJsonContext.Default.Person))
            .Message.ShouldContain("merge keys");
    }

    [Fact]
    public void RejectsMultipleDocuments()
    {
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("""
            Name: John
            ---
            Name: Jane
            """, AppJsonContext.Default.Person))
            .Message.ShouldContain("Multiple YAML documents");
    }

    [Fact]
    public void RejectsComplexMappingKeys()
    {
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("""
            ? [1, 2]
            : value
            """, AppJsonContext.Default.JsonElement))
            .Message.ShouldContain("Complex mapping keys");
    }

    [Fact]
    public void RejectsCustomTags()
    {
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("value: !Custom 123", AppJsonContext.Default.JsonElement))
            .Message.ShouldContain("not supported");
    }

    [Fact]
    public void RejectsInvalidYaml()
    {
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("""
            Name: John
             Broken: [unclosed
            """, AppJsonContext.Default.Person));
    }

    [Fact]
    public void RejectsEmptyInput()
    {
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("", AppJsonContext.Default.Person));
        Should.Throw<YamlException>(() => YamlSerializer.Deserialize("   ", AppJsonContext.Default.Person));
    }

    [Fact]
    public void DeserializesExplicitDocumentStart()
    {
        var yaml = YamlSerializer.Deserialize("""
            ---
            Name: John
            Age: 30
            """, AppJsonContext.Default.Person);

        yaml.ShouldNotBeNull();
        yaml.Name.ShouldBe("John");
    }

    [Fact]
    public void SupportsStreamOverloads()
    {
        var person = new Person { Name = "John", Age = 30 };

        using var serializeStream = new MemoryStream();
        YamlSerializer.Serialize(serializeStream, person, AppJsonContext.Default.Person);
        serializeStream.Flush();

        serializeStream.Position = 0;
        using var reader = new StreamReader(serializeStream, leaveOpen: true);
        var text = reader.ReadToEnd();

        text.ShouldBe("Name: John" + Environment.NewLine + "Age: 30" + Environment.NewLine);

        serializeStream.Position = 0;
        var result = YamlSerializer.Deserialize(serializeStream, AppJsonContext.Default.Person);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("John");
        result.Age.ShouldBe(30);
    }

    [Fact]
    public void StreamOverloadsLeaveTheStreamOpen()
    {
        var person = new Person { Name = "John", Age = 30 };

        using var stream = new MemoryStream();
        YamlSerializer.Serialize(stream, person, AppJsonContext.Default.Person);

        stream.CanWrite.ShouldBeTrue();

        stream.Position = 0;
        YamlSerializer.Deserialize(stream, AppJsonContext.Default.Person);

        stream.CanRead.ShouldBeTrue();
    }

    [Fact]
    public void RejectsNullTypeInfo()
    {
        Should.Throw<ArgumentNullException>(() => YamlSerializer.Serialize(new Person(), (JsonTypeInfo<Person>)null!));
        Should.Throw<ArgumentNullException>(() => YamlSerializer.Deserialize("Name: John", (JsonTypeInfo<Person>)null!));
    }

    [Fact]
    public void HandlesLargeIntegerScalarsWithoutLosingPrecision()
    {
        var yaml = "value: 123456789012345678901234567890";
        var element = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.JsonElement);

        element.GetProperty("value").ValueKind.ShouldBe(JsonValueKind.Number);
        element.GetProperty("value").GetRawText().ShouldBe("123456789012345678901234567890");
    }

    [Fact]
    public void ResolvesDuplicateKeysByUsingTheLastValue()
    {
        var element = YamlSerializer.Deserialize("""
            Name: John
            Name: Jane
            """, AppJsonContext.Default.Person);

        element!.Name.ShouldBe("Jane");
    }

    private static void YamlScalarShouldBe(string yaml, JsonValueKind kind, string? value = null, string? raw = null)
    {
        var element = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.JsonElement).GetProperty("value");

        element.ValueKind.ShouldBe(kind, $"YAML '{yaml}'");

        if (value != null)
        {
            element.GetString().ShouldBe(value, $"YAML '{yaml}'");
        }

        if (raw != null)
        {
            element.GetRawText().ShouldBe(raw, $"YAML '{yaml}'");
        }
    }
}
