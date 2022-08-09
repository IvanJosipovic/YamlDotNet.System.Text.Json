using System.Xml;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace YamlDotNet.System.Text.Json.Tests
{
    public class JsonNodeYamlConverterTests
    {
        [Theory]
        [InlineData("{\"Temperature\": \"25\"}")]
        //[InlineData("{\"Temperature\": 25}")]
        //[InlineData("{\"Temperatures\": [\"1\",\"2\",\"3\"]}")]
        //[InlineData("{\"Temperatures\": [1,2,3]}")]

        //[InlineData("25")]
        //[InlineData("\"25\"")]
        //[InlineData("test\ntest2\ntest3")]
        //[InlineData("true")]
        //[InlineData("false")]
        //[InlineData("0")]
        //[InlineData("100")]
        //[InlineData("1.55")]
        //[InlineData("\"1.55\"")]

        public void Tests(string val)
        {
            JsonNode val2 = JsonNode.Parse(val);

            string yaml = YamlConverter.Serialize(val2);

            //JsonNode val3 = YamlConverter.Deserialize(yaml);

            //Assert.Equal(val2.ToJsonString(), val3.ToJsonString());
        }
    }
}