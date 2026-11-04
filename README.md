# YamlDotNet.System.Text.Json

[![Nuget](https://img.shields.io/nuget/vpre/YamlDotNet.System.Text.Json.svg?style=flat-square)](https://www.nuget.org/packages/YamlDotNet.System.Text.Json)
[![Nuget)](https://img.shields.io/nuget/dt/YamlDotNet.System.Text.Json.svg?style=flat-square)](https://www.nuget.org/packages/YamlDotNet.System.Text.Json)
[![codecov](https://codecov.io/gh/IvanJosipovic/YamlDotNet.System.Text.Json/branch/main/graph/badge.svg?token=h453kfi3zo)](https://codecov.io/gh/IvanJosipovic/YamlDotNet.System.Text.Json)
## What is this?

This project contains components which allow [YamlDotNet](https://github.com/aaubry/YamlDotNet) to handle System.Text.Json objects and serialize them to YAML and back.

Supported Objects:

- [System.Text.Json.Nodes.JsonNode](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonnode)
- [System.Text.Json.Nodes.JsonArray](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonarray)
- [System.Text.Json.Nodes.JsonObject](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonobject)
- [System.Text.Json.Nodes.JsonValue](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonvalue)
- [System.Text.Json.JsonElement](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsonelement)
- [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument)
- [System.Text.Json.Serialization.JsonIgnoreAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonignoreattribute)
  - Conditions - controls whether a property is included while serializing and deserializing
    - Always             = Ignore during serialization and deserialization (default)
    - Never              = Include during serialization and deserialization
    - WhenWritingNull    = Ignore during serialization when the value is `null`
    - WhenWritingDefault = Ignore during serialization when the value is the type's default value
    - WhenWriting        = Ignore during serialization
    - WhenReading        = Ignore during deserialization

  Conditional `WhenWritingNull` and `WhenWritingDefault` properties remain readable during deserialization. Properties ignored with `Always` or `WhenReading` are treated as known ignored properties and skipped when they appear in YAML input.
- [System.Text.Json.Serialization.JsonPropertyNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonpropertynameattribute)
  - Name - Specifies the property name that is present in the JSON/YAML when serializing and deserializing.
- [System.Text.Json.Serialization.JsonPropertyOrderAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonpropertyorderattribute)
  - Order - Sets the serialization order of the property.
- [System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonstringenummembernameattribute)
  - Name - Sets the value for the Enum Member that is present in the JSON/YAML when serializing and deserializing.
- [System.Text.Json.Serialization.JsonExtensionDataAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonextensiondataattribute)

## Installation

```dotnet add package YamlDotNet.System.Text.Json```

## YamlSerializer

**YamlSerializer** serializes and deserializes YAML using the source generated contracts (`JsonTypeInfo<T>`) that your application already produces for System.Text.Json. No separate YAML metadata, attributes or context registration are needed, and no reflection is used, so it works with Native AOT and trimming. All System.Text.Json converters, attributes and options keep working because the actual object mapping is delegated to `JsonSerializer`.

```csharp
using System.Text.Json.Serialization;
using YamlDotNet.System.Text.Json;

[JsonSerializable(typeof(Person))]
public partial class AppJsonContext : JsonSerializerContext;

public sealed class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

internal static class Program
{
    private static void Main()
    {
        var person = new Person
        {
            Name = "John",
            Age = 30,
        };

        // serialize to YAML using the same contract as JSON
        string yaml = YamlSerializer.Serialize(person, AppJsonContext.Default.Person);

        // deserialize from YAML using the same contract as JSON
        Person? result = YamlSerializer.Deserialize(yaml, AppJsonContext.Default.Person);

        // stream overloads (UTF-8, the stream is not closed)
        YamlSerializer.Serialize(stream, person, AppJsonContext.Default.Person);
        result = YamlSerializer.Deserialize(stream, AppJsonContext.Default.Person);
    }
}
```

The same context keeps working for JSON:

```csharp
var json = JsonSerializer.Serialize(person, AppJsonContext.Default.Person);
```

### YAML scalar semantics

Plain (unquoted) scalars are resolved with the YAML 1.2 core schema, which matches JSON semantics:

| YAML input | Resolved as |
|---|---|
| `null`, `Null`, `NULL`, `~`, empty | JSON `null` |
| `true`, `True`, `TRUE`, `false` | JSON boolean |
| `123`, `-4`, `+5` | JSON integer |
| `1.20`, `-0.5`, `1e3` | JSON number (parsed invariant, `decimal` first, then `double`) |
| `yes`, `no`, `on`, `off`, `0x1A`, `0o17`, `2026-01-01` | JSON string (YAML 1.1 only values are treated as strings) |
| `"true"`, `"00123"`, `'1.20'`, block scalars (`|`, `>`) | JSON string, always |

Quoted scalars are always strings. Numbers are written back to YAML with their exact JSON text representation, so values do not lose precision. Large integers beyond `long` are preserved as exact JSON numbers.

Strings that a YAML parser could misinterpret are quoted on output: a JSON string `"true"` is emitted as `'true'` so it round-trips as a string.

### Supported and unsupported YAML

Supported: mappings with string keys, sequences, plain/quoted/block scalars, comments (ignored), standard YAML tags (`!!str`, `!!int`, `!!float`, `!!bool`, `!!null`), duplicate keys (last value wins), explicit document start (`---`).

Unsupported (these fail with a `YamlException` instead of producing incorrect results):

- Anchors and aliases (`&anchor`, `*alias`), including merge keys (`<<`)
- Complex mapping keys (sequences or mappings used as keys)
- Custom tags (`!Custom`)
- Multiple documents in one input (only the first document would be valid; the input is rejected)
- `.nan`, `.inf` and `-.inf` scalars, which have no JSON number representation (quote them to treat them as strings)

### Limitations

- Source generated contexts must include metadata for the target types (the default `JsonSerializable` generation mode does). Types registered only for the serialization fast path may not support deserialization.
- YAML input nesting beyond `System.Text.Json`'s `MaxDepth` (64) fails with a `JsonException`.
- Benchmarks comparing the reflection, static context and contract based APIs live in `benchmarks/YamlDotNet.System.Text.Json.Benchmarks` and can be run with `dotnet run -c Release --project benchmarks/YamlDotNet.System.Text.Json.Benchmarks -- --filter *`.

## YamlConverter

**YamlConverter** - exposes Serialize() and Deserialize\<T>() methods. Prefer [YamlSerializer](#yamlserializer) when a source generated System.Text.Json context is available; `YamlConverter` uses YamlDotNet's reflection or static context based pipelines.

```csharp
// to serialize a object to yaml
var yaml = YamlConverter.Serialize(someObject);

// to serialize json to yaml
var yaml = YamlConverter.SerializeJson(someJson);

// to load your object as a typed object
var obj = YamlConverter.Deserialize<MyTypedObject>(yaml);
```

## How to integrate with YamlDotNet

Example:

```csharp
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

var serializer = new SerializerBuilder()
            .AddSystemTextJson()
            .Build();

var yaml = serializer.Serialize(obj);

var deserializer = new DeserializerBuilder()
            .AddSystemTextJson()
            .Build();

var myObject = deserializer.Deserialize<MyType>(yaml)
```

### Trimming

Use the generated `StaticContext` overloads for trimmed or Native AOT apps. Install the generator with `dotnet add package Vecc.YamlDotNet.Analyzers.StaticGenerator`, register every model and enum, and preserve members that carry System.Text.Json attributes:

Nested model types require a generator package release that includes the nested-type-name fix. The published 18.1.0 generator does not include that fix, and no fixed minimum release is available yet.

```csharp
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.System.Text.Json;

[YamlStaticContext]
[YamlSerializable(typeof(ServiceConfig))]
[YamlSerializable(typeof(DeploymentMode))]
public partial class AppYamlContext : StaticContext
{
}

public sealed class ServiceConfig
{
    [JsonPropertyName("service-name")]
    public string Name { get; set; } = "api";

    public DeploymentMode Mode { get; set; }
}

public enum DeploymentMode
{
    [JsonStringEnumMemberName("production")]
    Production,
}

internal static class Program
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ServiceConfig))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicFields, typeof(DeploymentMode))]
    private static void Main()
    {
        var context = new AppYamlContext();
        var yaml = YamlConverter.Serialize(new ServiceConfig { Name = "api" }, context);
        var config = YamlConverter.Deserialize<ServiceConfig>(yaml, context);
    }
}
```

`AddSystemTextJson()` follows System.Text.Json field inclusion rules: public fields are ignored unless marked with `[JsonInclude]`. This changes the previous behavior for models that relied on YamlDotNet's default public-field handling.

To use YamlDotNet's builder APIs with the same context:

```csharp
var serializer = new StaticSerializerBuilder(context)
    .AddSystemTextJson()
    .Build();
var deserializer = new StaticDeserializerBuilder(context)
    .AddSystemTextJson()
    .Build();

var yaml = serializer.Serialize(config);
var roundTripped = deserializer.Deserialize<ServiceConfig>(yaml);
```

Set `<PublishAot>true</PublishAot>` in the app project for Native AOT, or `<PublishTrimmed>true</PublishTrimmed>` for trimming, then publish for your target runtime identifier. The context APIs use generated YamlDotNet model metadata; contextless `YamlConverter` methods and YamlDotNet's reflection builders require unreferenced code.
