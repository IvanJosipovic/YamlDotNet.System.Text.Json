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

## YamlConverter

**YamlConverter** - exposes Serialize() and Deserialize\<T>() methods

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

The library is trim-analyzed. Its default `YamlConverter` methods and `AddSystemTextJson` extensions for YamlDotNet's reflection based builders are marked as requiring unreferenced code. For trimmed applications, use the `YamlConverter` overloads that accept a generated `StaticContext`, or YamlDotNet's static builders.

Add the `Vecc.YamlDotNet.Analyzers.StaticGenerator` package to the consuming application, declare a partial `StaticContext` with `[YamlStaticContext]` and `[YamlSerializable(typeof(MyModel))]`, then pass that context to `YamlConverter.Serialize` and `YamlConverter.Deserialize<T>`. Register all application model types used by the context, including enum types. The `AddSystemTextJson` builder extensions also support YamlDotNet's static builder types.

The test project contains capability-specific shared fixtures and exercises the supported `JsonNode`, `JsonArray`, `JsonObject`, `JsonValue`, `JsonElement`, and `JsonDocument` types, as well as JSON attributes, extension data, naming, ordering, defaults, ignore conditions, enum names, and unmatched properties. CI publishes a focused xUnit Native AOT suite that covers the comprehensive fixture through a generated static context and a source-generated `JsonTypeInfo<T>` converter for included fields and `JsonElement` extension data.

`SystemTextJsonTypeInspector` reads `System.Text.Json` attributes from model metadata at runtime. A generated YAML static context registers the model descriptors, but trimmed applications must also retain those attributes, for example with `[DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MyModel))]` or by rooting the model assembly. The Native AOT tests apply targeted `DynamicDependency` roots to their registered fixture models.

For applications configuring YamlDotNet's static builders directly, register each model type and enum in the generated static context. The context must include public model properties, fields, and constructors, and enum public fields for `JsonStringEnumMemberNameAttribute`. For `IDictionary<string, JsonElement>` extension data, values may be JSON scalars, dictionaries with string keys, sequences, or existing `JsonElement`, `JsonDocument`, or `JsonNode` instances. Arbitrary CLR objects require runtime JSON metadata and are rejected. `IDictionary<string, object>` extension data retains its values as objects.

Extension-data properties using a concrete dictionary type other than `Dictionary<string, object>` or `Dictionary<string, JsonElement>` should be initialized before deserialization.

### Inspired By

[https://github.com/tomlm/YamlConvert](https://github.com/tomlm/YamlConvert)
