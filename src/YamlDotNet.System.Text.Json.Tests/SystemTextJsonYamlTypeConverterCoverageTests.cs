using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class SystemTextJsonYamlTypeConverterCoverageTests
{
    private static readonly ObjectDeserializer RootDeserializer = _ => null;
    private static readonly ObjectSerializer StubSerializer = (_, _) => { };

    [Fact]
    public void ReadYaml_ReturnsNullForUnsupportedType()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var parser = CreateParser("value");

        var result = converter.ReadYaml(parser, typeof(Guid), RootDeserializer);

        result.ShouldBeNull();
    }

    [Fact]
    public void ReadYaml_ReturnsNullForJsonObjectWhenScalarProvided()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var parser = CreateParser("scalar");

        var result = converter.ReadYaml(parser, typeof(JsonObject), RootDeserializer);

        result.ShouldBeNull();
    }

    [Fact]
    public void ReadYaml_ReturnsNullForJsonNodeWhenEmpty()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var parser = CreateParser(string.Empty);

        var result = converter.ReadYaml(parser, typeof(JsonNode), RootDeserializer);

        result.ShouldBeNull();
    }

    [Fact]
    public void ReadYaml_ReturnsNullForJsonDocumentWhenEmpty()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var parser = CreateParser(string.Empty);

        var result = converter.ReadYaml(parser, typeof(JsonDocument), RootDeserializer);

        result.ShouldBeNull();
    }

    [Fact]
    public void WriteYaml_HandlesJsonNodeValues()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var emitter = new RecordingEmitter();

        converter.WriteYaml(emitter, JsonNode.Parse("{\"name\":1}")!, typeof(JsonNode), StubSerializer);
        converter.WriteYaml(emitter, JsonNode.Parse("[1,2]")!, typeof(JsonNode), StubSerializer);
        converter.WriteYaml(emitter, JsonNode.Parse("\"text\"")!, typeof(JsonNode), StubSerializer);

        emitter.Events.OfType<MappingStart>().ShouldNotBeEmpty();
        emitter.Events.OfType<SequenceStart>().ShouldNotBeEmpty();
        emitter.Events.OfType<Scalar>().Any(s => s.Value == "text").ShouldBeTrue();
    }

    [Fact]
    public void WriteYaml_HandlesJsonElementKinds()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var emitter = new RecordingEmitter();

        using var objectDoc = JsonDocument.Parse("{\"value\":1}");
        converter.WriteYaml(emitter, objectDoc.RootElement, typeof(JsonElement), StubSerializer);

        using var arrayDoc = JsonDocument.Parse("[1,2]");
        converter.WriteYaml(emitter, arrayDoc.RootElement, typeof(JsonElement), StubSerializer);

        using var stringDoc = JsonDocument.Parse("\"value\"");
        converter.WriteYaml(emitter, stringDoc.RootElement, typeof(JsonElement), StubSerializer);

        converter.WriteYaml(emitter, default(JsonElement), typeof(JsonElement), StubSerializer);

        emitter.Events.OfType<Scalar>().ShouldNotBeEmpty();
    }

    [Fact]
    public void WriteYaml_HandlesJsonValueKinds()
    {
        var converter = new SystemTextJsonYamlTypeConverter();
        var emitter = new RecordingEmitter();

        var stringValue = JsonValue.Create("value")!;
        converter.WriteYaml(emitter, stringValue, typeof(JsonValue), StubSerializer);

        var numberValue = JsonValue.Create(3.14)!;
        converter.WriteYaml(emitter, numberValue, typeof(JsonValue), StubSerializer);

        var boolValue = JsonValue.Create(true)!;
        converter.WriteYaml(emitter, boolValue, typeof(JsonValue), StubSerializer);

        var nullValue = JsonValue.Create((string?)null)!;
        converter.WriteYaml(emitter, nullValue, typeof(JsonValue), StubSerializer);

        var undefinedValue = (JsonValue)JsonValue.Create(default(JsonElement))!;
        converter.WriteYaml(emitter, undefinedValue, typeof(JsonValue), StubSerializer);

        emitter.Events.Count.ShouldBeGreaterThan(0);
    }

    private static Parser CreateParser(string yaml)
    {
        var reader = new Parser(new StringReader(yaml ?? string.Empty));
        return reader;
    }

    private sealed class RecordingEmitter : IEmitter
    {
        public IList<ParsingEvent> Events { get; } = new List<ParsingEvent>();

        public void Emit(ParsingEvent @event)
        {
            Events.Add(@event);
        }
    }
}
