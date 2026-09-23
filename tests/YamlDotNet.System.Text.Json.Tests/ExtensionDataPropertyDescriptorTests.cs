using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using YamlDotNet.Core;

namespace YamlDotNet.System.Text.Json.Tests;

public class ExtensionDataPropertyDescriptorTests
{
    [Fact]
    public void PropertiesExposeMutableState()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(IDictionary<string, JsonElement>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        descriptor.AllowNulls = true;
        descriptor.TypeOverride = typeof(int);
        descriptor.ConverterType = typeof(string);
        descriptor.Order = 7;
        descriptor.ScalarStyle = ScalarStyle.DoubleQuoted;
        descriptor.CanWrite = true;

        descriptor.AllowNulls.ShouldBeTrue();
        descriptor.Name.ShouldBe("ExtensionData");
        descriptor.Required.ShouldBeFalse();
        descriptor.Type.ShouldBe(typeof(object));
        descriptor.TypeOverride.ShouldBe(typeof(int));
        descriptor.ConverterType.ShouldBe(typeof(string));
        descriptor.Order.ShouldBe(7);
        descriptor.ScalarStyle.ShouldBe(ScalarStyle.DoubleQuoted);
        descriptor.CanWrite.ShouldBeTrue();
    }

    [Fact]
    public void WriteSerializesJsonElementValues()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(Dictionary<string, JsonElement>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        var target = new object();

        descriptor.Write(target, "value");

        var stored = (IDictionary<string, JsonElement>)baseDescriptor.Read(target).Value!;
        stored.ShouldContainKey("ExtensionData");
        stored["ExtensionData"].ValueKind.ShouldBe(JsonValueKind.String);
        stored["ExtensionData"].GetString().ShouldBe("value");

        var roundTrip = descriptor.Read(target);
        roundTrip.Value.ShouldBe(stored["ExtensionData"]);
    }

    [Fact]
    public void WriteCreatesDictionaryForObjectInterface()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(IDictionary<string, object>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        var target = new object();

        descriptor.Write(target, 42);

        var stored = (IDictionary<string, object>)baseDescriptor.Read(target).Value!;
        stored.ShouldContainKey("ExtensionData");
        stored["ExtensionData"].ShouldBe(42);
    }

    [Fact]
    public void WriteCreatesDictionaryForJsonElementInterface()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(IDictionary<string, JsonElement>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        var target = new object();

        descriptor.Write(target, "payload");

        var stored = (IDictionary<string, JsonElement>)baseDescriptor.Read(target).Value!;
        stored.ShouldContainKey("ExtensionData");
        stored["ExtensionData"].GetString().ShouldBe("payload");
    }

    [Fact]
    public void WriteCreatesConcreteObjectDictionary()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(Dictionary<string, object>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        descriptor.Write(new object(), 42);

        var stored = (Dictionary<string, object>)baseDescriptor.Read(null).Value!;
        stored["ExtensionData"].ShouldBe(42);
    }

    [Fact]
    public void WriteCreatesConcreteJsonElementDictionary()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(Dictionary<string, JsonElement>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        descriptor.Write(new object(), "payload");

        var stored = (Dictionary<string, JsonElement>)baseDescriptor.Read(null).Value!;
        stored["ExtensionData"].GetString().ShouldBe("payload");
    }

    [Fact]
    public void WriteThrowsForUnsupportedDictionaryValueType()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(Dictionary<string, int>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        var target = new object();

        Should.Throw<InvalidOperationException>(() => descriptor.Write(target, 5));
    }

    [Fact]
    public void WriteThrowsWhenTypeIsNotDictionary()
    {
        var baseDescriptor = new TestPropertyDescriptor("ExtensionData", typeof(string));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        var target = new object();

        Should.Throw<InvalidOperationException>(() => descriptor.Write(target, "value"));
    }

    [Theory]
    [MemberData(nameof(JsonScalarValues))]
    public void WriteConvertsSupportedJsonScalarsToJsonElement(object? value, string expectedJson)
    {
        var baseDescriptor = new TestPropertyDescriptor("value", typeof(IDictionary<string, JsonElement>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        descriptor.Write(new object(), value);

        var dictionary = (IDictionary<string, JsonElement>)baseDescriptor.Read(null).Value!;
        dictionary["value"].GetRawText().ShouldBe(expectedJson);
    }

    public static TheoryData<object?, string> JsonScalarValues => new()
    {
        { null, "null" },
        { "text", "\"text\"" },
        { 'x', "\"x\"" },
        { true, "true" },
        { new byte[] { 1, 2 }, "\"AQI=\"" },
        { (byte)1, "1" },
        { (sbyte)-1, "-1" },
        { (short)-2, "-2" },
        { (ushort)2, "2" },
        { -3, "-3" },
        { 3U, "3" },
        { -4L, "-4" },
        { 4UL, "4" },
        { 1.5F, "1.5" },
        { 2.5D, "2.5" },
        { 3.5M, "3.5" },
        { new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc), "\"2020-01-02T03:04:05Z\"" },
        { new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero), "\"2020-01-02T03:04:05+00:00\"" },
        { Guid.Parse("00112233-4455-6677-8899-aabbccddeeff"), "\"00112233-4455-6677-8899-aabbccddeeff\"" },
        { SignedEnum.Negative, "-1" },
        { UnsignedEnum.Maximum, "18446744073709551615" },
    };

    [Fact]
    public void WriteConvertsJsonDomAndNestedCollections()
    {
        using var document = JsonDocument.Parse("{\"doc\":true}");
        var value = new Dictionary<string, object?>
        {
            ["element"] = JsonDocument.Parse("[1]").RootElement,
            ["document"] = document,
            ["node"] = JsonNode.Parse("\"node\""),
            ["nested"] = new object?[] { null, 2, new Dictionary<string, object> { ["ok"] = true } },
        };
        var baseDescriptor = new TestPropertyDescriptor("value", typeof(IDictionary<string, JsonElement>));
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        descriptor.Write(new object(), value);

        var json = ((IDictionary<string, JsonElement>)baseDescriptor.Read(null).Value!)["value"];
        json.GetProperty("element")[0].GetInt32().ShouldBe(1);
        json.GetProperty("document").GetProperty("doc").GetBoolean().ShouldBeTrue();
        json.GetProperty("node").GetString().ShouldBe("node");
        json.GetProperty("nested")[0].ValueKind.ShouldBe(JsonValueKind.Null);
        json.GetProperty("nested")[2].GetProperty("ok").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void WriteRejectsUnsupportedRuntimeValues()
    {
        var descriptor = new ExtensionDataPropertyDescriptor(new TestPropertyDescriptor("value", typeof(IDictionary<string, JsonElement>)));

        Should.Throw<InvalidOperationException>(() => descriptor.Write(new object(), new object()))
            .Message.ShouldContain("cannot be converted without runtime JSON metadata");
    }

    [Fact]
    public void WriteRejectsDictionariesWithNonStringKeys()
    {
        var descriptor = new ExtensionDataPropertyDescriptor(new TestPropertyDescriptor("value", typeof(IDictionary<string, JsonElement>)));
        var value = new Dictionary<int, object> { [1] = "value" };

        Should.Throw<InvalidOperationException>(() => descriptor.Write(new object(), value))
            .Message.ShouldContain("must have string keys");
    }

    [Fact]
    public void WriteUsesExistingExtensionDataDictionary()
    {
        var existing = new Dictionary<string, object> { ["existing"] = 1 };
        var baseDescriptor = new TestPropertyDescriptor("value", typeof(IDictionary<string, object>), existing);
        var descriptor = new ExtensionDataPropertyDescriptor(baseDescriptor);

        descriptor.Write(new object(), 2);

        baseDescriptor.Read(null).Value.ShouldBeSameAs(existing);
        existing["value"].ShouldBe(2);
    }

    [Fact]
    public void WriteRequiresCustomDictionaryToBeInitialized()
    {
        var descriptor = new ExtensionDataPropertyDescriptor(new TestPropertyDescriptor("value", typeof(CustomDictionary)));

        Should.Throw<InvalidOperationException>(() => descriptor.Write(new object(), 2))
            .Message.ShouldContain("Initialize the property before deserialization");
    }

    [Fact]
    public void WriteRequiresCustomJsonElementDictionaryToBeInitialized()
    {
        var descriptor = new ExtensionDataPropertyDescriptor(new TestPropertyDescriptor("value", typeof(CustomJsonElementDictionary)));

        Should.Throw<InvalidOperationException>(() => descriptor.Write(new object(), JsonDocument.Parse("null").RootElement))
            .Message.ShouldContain("Initialize the property before deserialization");
    }

    [Fact]
    public void GetCustomAttributeForwardsToBaseDescriptor()
    {
        var attribute = new JsonIgnoreAttribute();
        var descriptor = new ExtensionDataPropertyDescriptor(new TestPropertyDescriptor(
            "value", typeof(IDictionary<string, object>), attributes: new Attribute[] { attribute }));

        descriptor.GetCustomAttribute<JsonIgnoreAttribute>().ShouldBeSameAs(attribute);
        descriptor.GetCustomAttribute<JsonPropertyNameAttribute>().ShouldBeNull();
    }

    private enum SignedEnum : long
    {
        Negative = -1,
    }

    private enum UnsignedEnum : ulong
    {
        Maximum = ulong.MaxValue,
    }

    private sealed class CustomDictionary : Dictionary<string, object>
    {
    }

    private sealed class CustomJsonElementDictionary : Dictionary<string, JsonElement>
    {
    }
}
