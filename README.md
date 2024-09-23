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
  - Conditions - condition that must be met before a property will be ignored
    - Always             = Ignore (Default)
    - Never              = Serialize
    - WhenWritingNull    = Serialize
    - WhenWritingDefault = Serialize
- [System.Text.Json.Serialization.JsonPropertyNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonpropertynameattribute)
  - Name - Specifies the property name that is present in the JSON/YAML when serializing and deserializing.
- [System.Text.Json.Serialization.JsonPropertyOrderAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonpropertyorderattribute)
  - Order - Sets the serialization order of the property.
- [System.Text.Json.Serialization.JsonStringEnumMemberNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonstringenummembernameattribute)
  - Name - Sets the value for the Enum Member that is present in the JSON/YAML when serializing and deserializing.

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

## How to use manually

### SystemTextJsonYamlTypeConverter
This is a type converter for reading and writing System.Text.Json objects.

``` .WithTypeConverter(new SystemTextJsonYamlTypeConverter())```

### SystemTextJsonTypeInspector
This is a type inspector for reading System.Text.Json Attributes

``` .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))```

Example:

```csharp
var serializer = new SerializerBuilder()
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .Build();
var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x))
            .Build();
```

### Inspired By

[https://github.com/tomlm/YamlConvert](https://github.com/tomlm/YamlConvert)
