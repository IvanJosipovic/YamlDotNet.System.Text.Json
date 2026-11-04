using System.Text.Json;
using System.Text.Json.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public partial class YamlSerializerCompatibilityTests
{
    public sealed class AttributedModel
    {
        [JsonPropertyName("display-name")]
        public string Name { get; set; } = "";

        [JsonIgnore]
        public string Secret { get; set; } = "";

        [JsonPropertyOrder(2)]
        public string Last { get; set; } = "";

        [JsonPropertyOrder(1)]
        public string First { get; set; } = "";

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? Extra { get; set; }
    }

    public sealed class RequiredModel
    {
        [JsonRequired]
        public string Name { get; set; } = "";
    }

    public sealed class NumberHandlingModel
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Port { get; set; }
    }

    public sealed class UpperCaseString
    {
        [JsonConverter(typeof(UpperCaseStringConverter))]
        public string Value { get; set; } = "";
    }

    public sealed class UpperCaseStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString()!.ToUpperInvariant();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToUpperInvariant());
        }
    }

    public record RecordModel(string FirstName, string LastName);

    [JsonPolymorphic]
    [JsonDerivedType(typeof(Dog), "dog")]
    [JsonDerivedType(typeof(Cat), "cat")]
    public abstract class Animal
    {
        public string Name { get; set; } = "";
    }

    public sealed class Dog : Animal
    {
        public int BarkVolume { get; set; }
    }

    public sealed class Cat : Animal
    {
        public bool Indoor { get; set; }
    }

    public sealed class NullableModel
    {
        public int? Maybe { get; set; }
    }

    public enum Status
    {
        Active,
        Inactive,
    }

    public sealed class EnumModel
    {
        public Status Status { get; set; }
    }

    [JsonSourceGenerationOptions(UseStringEnumConverter = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip)]
    [JsonSerializable(typeof(AttributedModel))]
    [JsonSerializable(typeof(RequiredModel))]
    [JsonSerializable(typeof(NumberHandlingModel))]
    [JsonSerializable(typeof(UpperCaseString))]
    [JsonSerializable(typeof(RecordModel))]
    [JsonSerializable(typeof(List<Animal>))]
    [JsonSerializable(typeof(Animal))]
    [JsonSerializable(typeof(NullableModel))]
    [JsonSerializable(typeof(EnumModel))]
    [JsonSerializable(typeof(Dictionary<string, List<string>>))]
    [JsonSerializable(typeof(GenericWrapper<AttributedModel>))]
    internal sealed partial class CompatContext : JsonSerializerContext;

    public sealed class GenericWrapper<T>
    {
        public T? Value { get; set; }
    }

    [Fact]
    public void SupportsJsonPropertyName()
    {
        var model = new AttributedModel { Name = "api" };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.AttributedModel);

        yaml.ShouldContain("display-name: api");

        var result = YamlSerializer.Deserialize("display-name: api2" + Environment.NewLine, CompatContext.Default.AttributedModel);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("api2");
    }

    [Fact]
    public void SupportsJsonIgnore()
    {
        var model = new AttributedModel { Name = "api", Secret = "hidden" };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.AttributedModel);

        yaml.ShouldNotContain("hidden");
        yaml.ShouldNotContain("Secret");
    }

    [Fact]
    public void SupportsJsonPropertyOrder()
    {
        var model = new AttributedModel { Name = "api", First = "one", Last = "two" };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.AttributedModel);

        yaml.IndexOf("first", StringComparison.Ordinal).ShouldBeLessThan(yaml.IndexOf("last", StringComparison.Ordinal));
    }

    [Fact]
    public void SupportsJsonExtensionData()
    {
        var model = new AttributedModel { Name = "api", Extra = new Dictionary<string, JsonElement> { ["custom"] = JsonDocument.Parse("\"hello\"").RootElement } };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.AttributedModel);

        yaml.ShouldContain("custom: hello");

        var result = YamlSerializer.Deserialize(yaml, CompatContext.Default.AttributedModel);

        result.ShouldNotBeNull();
        result.Extra.ShouldNotBeNull();
        result.Extra["custom"].GetString().ShouldBe("hello");
    }

    [Fact]
    public void SupportsJsonRequired()
    {
        var model = new RequiredModel { Name = "api" };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.RequiredModel);
        var result = YamlSerializer.Deserialize(yaml, CompatContext.Default.RequiredModel);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("api");

        Should.Throw<JsonException>(() => YamlSerializer.Deserialize("{}", CompatContext.Default.RequiredModel));
    }

    [Fact]
    public void SupportsPropertyNamingPolicy()
    {
        var model = new RequiredModel { Name = "api" };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.RequiredModel);

        yaml.ShouldContain("name: api");
    }

    [Fact]
    public void SupportsJsonNumberHandling()
    {
        var result = YamlSerializer.Deserialize("port: \"8080\"" + Environment.NewLine, CompatContext.Default.NumberHandlingModel);

        result.ShouldNotBeNull();
        result.Port.ShouldBe(8080);
    }

    [Fact]
    public void SupportsCustomConverters()
    {
        var model = new UpperCaseString { Value = "hello" };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.UpperCaseString);

        yaml.ShouldContain("value: HELLO");

        var result = YamlSerializer.Deserialize("value: world" + Environment.NewLine, CompatContext.Default.UpperCaseString);

        result.ShouldNotBeNull();
        result.Value.ShouldBe("WORLD");
    }

    [Fact]
    public void SupportsConstructorParameterBinding()
    {
        var model = new RecordModel("John", "Doe");

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.RecordModel);

        yaml.ShouldBe("firstName: John" + Environment.NewLine + "lastName: Doe" + Environment.NewLine);

        var result = YamlSerializer.Deserialize(yaml, CompatContext.Default.RecordModel);

        result.ShouldBe(model);
    }

    [Fact]
    public void SupportsStringEnums()
    {
        var yaml = YamlSerializer.Serialize(new EnumModel { Status = Status.Active }, CompatContext.Default.EnumModel);

        yaml.ShouldContain("status: Active");

        var result = YamlSerializer.Deserialize("status: inactive" + Environment.NewLine, CompatContext.Default.EnumModel);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(Status.Inactive);
    }

    [Fact]
    public void SupportsPolymorphism()
    {
        var animals = new List<Animal>
        {
            new Dog { Name = "Rex", BarkVolume = 11 },
            new Cat { Name = "Tom", Indoor = true },
        };

        var yaml = YamlSerializer.Serialize(animals, CompatContext.Default.ListAnimal);
        var result = YamlSerializer.Deserialize(yaml, CompatContext.Default.ListAnimal);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result[0].ShouldBeOfType<Dog>().BarkVolume.ShouldBe(11);
        result[1].ShouldBeOfType<Cat>().Indoor.ShouldBeTrue();
    }

    [Fact]
    public void SupportsNullableValueTypes()
    {
        var model = new NullableModel { Maybe = 7 };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.NullableModel);
        var result = YamlSerializer.Deserialize(yaml, CompatContext.Default.NullableModel);

        result.ShouldNotBeNull();
        result.Maybe.ShouldBe(7);

        var none = new NullableModel();
        var yamlNone = YamlSerializer.Serialize(none, CompatContext.Default.NullableModel);
        var resultNone = YamlSerializer.Deserialize(yamlNone, CompatContext.Default.NullableModel);

        resultNone.ShouldNotBeNull();
        resultNone.Maybe.ShouldBeNull();
    }

    [Fact]
    public void SupportsGenericModels()
    {
        var model = new GenericWrapper<AttributedModel> { Value = new AttributedModel { Name = "wrapped" } };

        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.GenericWrapperAttributedModel);
        var result = YamlSerializer.Deserialize(yaml, CompatContext.Default.GenericWrapperAttributedModel);

        result.ShouldNotBeNull();
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("wrapped");
    }

    [Fact]
    public void SupportsUnmappedMemberHandling()
    {
        var result = YamlSerializer.Deserialize("""
            name: api
            unknown: ignored
            """, CompatContext.Default.RequiredModel);

        result.ShouldNotBeNull();
        result.Name.ShouldBe("api");
    }

    [Fact]
    public void JsonAndYamlDeserializeToEqualValues()
    {
        var model = new AttributedModel { Name = "api", First = "one", Last = "two", Extra = new Dictionary<string, JsonElement> { ["x"] = JsonDocument.Parse("42").RootElement } };

        var json = JsonSerializer.Serialize(model, CompatContext.Default.AttributedModel);
        var yaml = YamlSerializer.Serialize(model, CompatContext.Default.AttributedModel);

        var fromJson = JsonSerializer.Deserialize(json, CompatContext.Default.AttributedModel);
        var fromYaml = YamlSerializer.Deserialize(yaml, CompatContext.Default.AttributedModel);

        fromJson.ShouldNotBeNull();
        fromYaml.ShouldNotBeNull();
        fromYaml.Name.ShouldBe(fromJson.Name);
        fromYaml.First.ShouldBe(fromJson.First);
        fromYaml.Last.ShouldBe(fromJson.Last);
        fromYaml.Extra.ShouldNotBeNull();
        fromYaml.Extra["x"].GetInt32().ShouldBe(42);
    }
}
