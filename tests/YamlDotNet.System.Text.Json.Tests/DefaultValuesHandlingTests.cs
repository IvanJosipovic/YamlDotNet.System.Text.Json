using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class DefaultValuesHandlingTests
{
    [Fact]
    public void Serialize_PreservesDefaultValuesByDefault()
    {
        var model = new FixtureModels.DefaultValues.Model
        {
            NullableString = null,
            Number = 0,
            Numbers = new List<int>()
        };

        var yaml = StaticYaml.Serialize(model);

        yaml.ShouldContain("NullableString");
        yaml.ShouldContain("Number: 0");
        yaml.ShouldContain("Numbers: []");
    }

    [Fact]
    public void Serialize_OmitsNullValuesWhenConfigured()
    {
        var model = new FixtureModels.DefaultValues.Model
        {
            NullableString = null,
            Number = 5,
            Numbers = new List<int> { 1 }
        };

        var yaml = StaticYaml.Serialize(model, defaultValuesHandling: DefaultValuesHandling.OmitNull);

        yaml.ShouldNotContain("NullableString");
        yaml.ShouldContain("Number: 5");
        yaml.ShouldContain("Numbers:");
    }

    [Fact]
    public void Serialize_OmitsDefaultValuesWhenConfigured()
    {
        var model = new FixtureModels.DefaultValues.Model
        {
            NullableString = "value",
            Number = 0,
            Numbers = new List<int> { 1 }
        };

        var yaml = StaticYaml.Serialize(model, defaultValuesHandling: DefaultValuesHandling.OmitDefaults);

        yaml.ShouldContain("NullableString: value");
        yaml.ShouldNotContain("Number:");
        yaml.ShouldContain("Numbers:");
    }

    [Fact]
    public void Serialize_OmitsEmptyCollectionsWhenConfigured()
    {
        var model = new FixtureModels.DefaultValues.Model
        {
            NullableString = "value",
            Number = 5,
            Numbers = new List<int>()
        };

        var yaml = StaticYaml.Serialize(model, defaultValuesHandling: DefaultValuesHandling.OmitEmptyCollections);

        yaml.ShouldContain("NullableString: value");
        yaml.ShouldContain("Number: 5");
        yaml.ShouldNotContain("Numbers:");
    }
}
