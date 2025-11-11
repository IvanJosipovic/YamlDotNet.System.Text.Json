using System.Text.Json;
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
}
