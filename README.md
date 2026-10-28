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

Use the generated `StaticContext` overloads for trimmed or Native AOT apps. Install the generator with `dotnet add package Vecc.YamlDotNet.Analyzers.StaticGenerator`, register every model and enum, and preserve members that carry System.Text.Json attributes:

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
