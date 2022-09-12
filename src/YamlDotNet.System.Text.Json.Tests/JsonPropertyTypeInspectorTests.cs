using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace YamlDotNet.System.Text.Json.Tests
{
    public class JsonPropertyTypeInspectorTests
    {
        public class TestModel
        {
            [JsonPropertyName("MyNewPropName")]
            public string MyProp { get; set; }

            public string MyProp2 { get; set; }

            [JsonIgnore()]
            public string Hide { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
            public string Hide2 { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public string Show { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public string Show2 { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
            public string Show3 { get; set; }
        }

        public class TestModel2
        {
            [JsonPropertyOrder(3)]
            public string MyProp { get; set; }

            [JsonPropertyOrder(2)]
            public string MyProp2 { get; set; }

            [JsonPropertyOrder(1)]
            public string MyProp3 { get; set; }
        }

        [Fact]
        public void Serialize()
        {
            var model = new TestModel()
            {
                MyProp = nameof(TestModel.MyProp),
                MyProp2 = nameof(TestModel.MyProp2),
                Hide = nameof(TestModel.Hide),
                Hide2 = nameof(TestModel.Hide2),
                Show = nameof(TestModel.Show),
                Show2 = nameof(TestModel.Show2),
                Show3 = nameof(TestModel.Show3),
            };

            var yaml = YamlConverter.Serialize(model);

            string expected = """
                              MyNewPropName: MyProp
                              MyProp2: MyProp2
                              Show: Show
                              Show2: Show2
                              Show3: Show3

                              """;
            yaml.Should().Be(expected);
        }

        [Fact]
        public void Deserialize()
        {
            var yaml = """
                        MyNewPropName: test
                        MyProp2: test2
                        Show: test5
                        Show2: test6
                        Show3: test7

                        """;

            var model = YamlConverter.Deserialize<TestModel>(yaml);

            model.MyProp.Should().Be("test");
            model.MyProp2.Should().Be("test2");
            model.Show.Should().Be("test5");
            model.Show2.Should().Be("test6");
            model.Show3.Should().Be("test7");
        }

        [Fact]
        public void Order()
        {
            var model = new TestModel2()
            {
                MyProp = nameof(TestModel2.MyProp),
                MyProp2 = nameof(TestModel2.MyProp2),
                MyProp3 = nameof(TestModel2.MyProp3),
            };

            var yaml = YamlConverter.Serialize(model);

            string expected = """
                              MyProp3: MyProp3
                              MyProp2: MyProp2
                              MyProp: MyProp

                              """;
            yaml.Should().Be(expected);
        }

        [Fact]
        public void DisableOrder()
        {
            ISerializer serializer = new SerializerBuilder()
            .DisableAliases()
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .WithTypeConverter(new SystemTextJsonYamlTypeConverter())
            .WithTypeInspector(x => new SystemTextJsonTypeInspector(x, true))
            .Build();

        var model = new TestModel2()
            {
                MyProp = nameof(TestModel2.MyProp),
                MyProp2 = nameof(TestModel2.MyProp2),
                MyProp3 = nameof(TestModel2.MyProp3),
            };

            var yaml = YamlConverter.Serialize(model, serializer);

            string expected = """
                              MyProp: MyProp
                              MyProp2: MyProp2
                              MyProp3: MyProp3

                              """;
            yaml.Should().Be(expected);
        }
    }
}
