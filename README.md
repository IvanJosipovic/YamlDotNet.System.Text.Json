# YamlDotNet.System.Text.Json

[![Nuget](https://img.shields.io/nuget/vpre/YamlDotNet.System.Text.Json.svg?style=flat-square)](https://www.nuget.org/packages/YamlDotNet.System.Text.Json)
[![Nuget)](https://img.shields.io/nuget/dt/YamlDotNet.System.Text.Json.svg?style=flat-square)](https://www.nuget.org/packages/YamlDotNet.System.Text.Json)
[![codecov](https://codecov.io/gh/IvanJosipovic/YamlDotNet.System.Text.Json/branch/alpha/graph/badge.svg?token=h453kfi3zo)](https://codecov.io/gh/IvanJosipovic/YamlDotNet.System.Text.Json)
## What is this?

This project contains a [IYamlTypeConverter](https://github.com/aaubry/YamlDotNet/wiki/Serialization.Serializer#withtypeconverteriyamltypeconverter) which can convert System.Text.Json objects to YAML and back.

Supported Objects:

- [System.Text.Json.Nodes.JsonNode](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonnode)
- [System.Text.Json.Nodes.JsonArray](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonarray)
- [System.Text.Json.Nodes.JsonObject](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonobject)
- [System.Text.Json.Nodes.JsonValue](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.nodes.jsonvalue)
- [System.Text.Json.JsonElement](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsonelement)
- [System.Text.Json.JsonDocument](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.jsondocument)

## Installation

```dotnet add package YamlDotNet.System.Text.Json --prerelease```

## YamlConverter

**YamlConverter** - exposes Serialize() and Deserialize\<T>() methods

```csharp
// to serialize to yaml
var yaml = YamlConverter.Serialize(someObject);

// to load your object as a typed object
var obj2 = YamlConverter.Deserialize<MyTypedObject>(yaml);
```

## SystemTextJsonYamlTypeConverter
This is a type converter for reading and writing System.Text.Json objects. It's automatically used by YamlConverter, but you can add it to your own serializer definition by using
``` .WithTypeConverter(new SystemTextJsonYamlTypeConverter())```

Example:

```csharp
var serializer = new SerializerBuilder()
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .Build();
var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .Build();
```

### Inspired By

[https://github.com/tomlm/YamlConvert](https://github.com/tomlm/YamlConvert)