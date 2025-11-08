using FluentAssertions;
using System.Text.Json.Serialization;
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

            public TestEnum? Enum { get; set; }

            public IList<TestEnum> EnumList { get; set; }
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

        public enum TestEnum
        {
            First,
            [JsonStringEnumMemberName("val2")]
            Second,
            Third
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
                Enum = TestEnum.Third,
                EnumList =
                [
                    TestEnum.Third
                ]
            };

            var yaml = YamlConverter.Serialize(model);

            string expected = """
                              MyNewPropName: MyProp
                              MyProp2: MyProp2
                              Show: Show
                              Show2: Show2
                              Show3: Show3
                              Enum: Third
                              EnumList:
                              - Third

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
                        Enum: Third
                        EnumList:
                        - Third

                        """;

            var model = YamlConverter.Deserialize<TestModel>(yaml);

            model.MyProp.Should().Be("test");
            model.MyProp2.Should().Be("test2");
            model.Show.Should().Be("test5");
            model.Show2.Should().Be("test6");
            model.Show3.Should().Be("test7");
            model.Enum.Should().Be(TestEnum.Third);
            model.EnumList[0].Should().Be(TestEnum.Third);
        }

        [Fact]
        public void PropertyOrder()
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
        public void DisablePropertyOrder()
        {
            var model = new TestModel2()
            {
                MyProp = nameof(TestModel2.MyProp),
                MyProp2 = nameof(TestModel2.MyProp2),
                MyProp3 = nameof(TestModel2.MyProp3),
            };

            var yaml = YamlConverter.Serialize(model, null, false, true);

            string expected = """
                              MyProp: MyProp
                              MyProp2: MyProp2
                              MyProp3: MyProp3

                              """;
            yaml.Should().Be(expected);
        }

        [Fact]
        public void JsonStringEnumMemberNameSerialize()
        {
            var model = new TestModel()
            {
                Enum = TestEnum.Second,
                EnumList =
                [
                    TestEnum.Second
                ]
            };

            var yaml = YamlConverter.Serialize(model);

            string expected = """
                              Enum: val2
                              EnumList:
                              - val2

                              """;
            yaml.Should().Be(expected);
        }

        [Fact]
        public void JsonStringEnumMemberNameDeserialize()
        {
            var yaml = """
                        Enum: val2
                        EnumList:
                        - val2

                        """;

            var model = YamlConverter.Deserialize<TestModel>(yaml);

            model.Enum.Should().Be(TestEnum.Second);
            model.EnumList[0].Should().Be(TestEnum.Second);
        }
    }
}
