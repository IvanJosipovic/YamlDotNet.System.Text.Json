using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests;

public class DefaultValuesHandlingTests
{
    private sealed class DefaultValuesModel
    {
        public string? NullableString { get; set; }

        public int Number { get; set; }

        public List<int> Numbers { get; set; } = new();
    }

    [Fact]
    public void Serialize_PreservesDefaultValuesByDefault()
    {
        var model = new DefaultValuesModel
        {
            NullableString = null,
            Number = 0,
            Numbers = new List<int>()
        };

        var yaml = YamlConverter.Serialize(model);

        yaml.ShouldContain("NullableString");
        yaml.ShouldContain("Number: 0");
        yaml.ShouldContain("Numbers: []");
    }

    [Fact]
    public void Serialize_OmitsNullValuesWhenConfigured()
    {
        var model = new DefaultValuesModel
        {
            NullableString = null,
            Number = 5,
            Numbers = new List<int> { 1 }
        };

        var yaml = YamlConverter.Serialize(model, defaultValuesHandling: DefaultValuesHandling.OmitNull);

        yaml.ShouldNotContain("NullableString");
        yaml.ShouldContain("Number: 5");
        yaml.ShouldContain("Numbers:");
    }

    [Fact]
    public void Serialize_OmitsDefaultValuesWhenConfigured()
    {
        var model = new DefaultValuesModel
        {
            NullableString = "value",
            Number = 0,
            Numbers = new List<int> { 1 }
        };

        var yaml = YamlConverter.Serialize(model, defaultValuesHandling: DefaultValuesHandling.OmitDefaults);

        yaml.ShouldContain("NullableString: value");
        yaml.ShouldNotContain("Number:");
        yaml.ShouldContain("Numbers:");
    }

    [Fact]
    public void Serialize_OmitsEmptyCollectionsWhenConfigured()
    {
        var model = new DefaultValuesModel
        {
            NullableString = "value",
            Number = 5,
            Numbers = new List<int>()
        };

        var yaml = YamlConverter.Serialize(model, defaultValuesHandling: DefaultValuesHandling.OmitEmptyCollections);

        yaml.ShouldContain("NullableString: value");
        yaml.ShouldContain("Number: 5");
        yaml.ShouldNotContain("Numbers:");
    }
}
