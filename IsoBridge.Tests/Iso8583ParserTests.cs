using IsoBridge.Core.Models;
using IsoBridge.ISO8583;
using Microsoft.Extensions.Options;
using Xunit;

namespace IsoBridge.Tests
{
    public class Iso8583ParserTests
    {
        private readonly DefaultIsoParser _parser;

        public Iso8583ParserTests()
        {
            var opts = Options.Create(new Iso8583Options
            {
                TemplatePath = Path.Combine("Config", "iso8583-templates.json"),
                UseBcd = false
            });

            _parser = new DefaultIsoParser(opts);
        }

        [Fact]
        public void Build_Then_Parse_Should_Return_Equivalent_Message()
        {
            var original = new IsoMessage("0200", new Dictionary<int, string>
            {
                [2] = "4242424242424242",
                [3] = "000000",
                [4] = "000000010000",
                [7] = "0318230000",
                [11] = "123456",
                [41] = "TERM001",
                [49] = "840"
            });

            // act
            var bytes = _parser.Build(original);
            var result = _parser.Parse(bytes);

            // assert
            Assert.True(result.Success, result.Error);
            Assert.Equal(original.Mti, result.Message!.Mti);

            foreach (var kv in original.Fields)
            {
                Assert.Equal(kv.Value, result.Message.Fields[kv.Key]);
            }
        }
    }
}