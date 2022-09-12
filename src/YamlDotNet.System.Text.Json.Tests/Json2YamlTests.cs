using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace YamlDotNet.System.Text.Json.Tests
{
    public class Json2YamlTests
    {
        [Fact]
        public void Test1()
        {
            var json = """
                       {
                        "Temperature": "25"
                       }
                       """;

            var yaml = YamlConverter.SerializeJson(json);

            string expected = """
                              Temperature: '25'

                              """;
            yaml.Should().Be(expected);
        }

    }
}
