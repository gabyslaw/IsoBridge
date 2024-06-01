using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace IsoBridge.Tests
{
    public class IsoBridgeIntegrationTests : IClassFixture<TestServerFixture>
    {
        private readonly HttpClient _client;

        public IsoBridgeIntegrationTests(TestServerFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task Parse_Should_Return_ValidJson()
        {
            var isoHex = "30313030F23C449108E180000000000000000016303030303030303031323334353637383930313233343536373839303132333435363738393031323334353637383930";
            var response = await _client.PostAsJsonAsync("/api/iso/parse", new { iso = isoHex });
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"mti\"", json);
            Assert.Contains("\"fields\"", json);
        }

        [Fact]
        public async Task Build_Should_Return_HexString()
        {
            var req = new
            {
                Mti = "0100",
                Fields = new Dictionary<string, string>
                {
                    ["2"] = "4111111111111111",
                    ["4"] = "000000001000",
                    ["41"] = "TERM001",
                    ["49"] = "840"
                }
            };

            var response = await _client.PostAsJsonAsync("/api/iso/build", req);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("hex", json, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Forward_Should_Work_To_SOAP()
        {
            var req = new
            {
                RouteKey = "soap-local",
                Mode = "json",
                Mti = "0100",
                Fields = new Dictionary<string, string>
                {
                    ["2"] = "4111111111111111",
                    ["4"] = "000000001000",
                    ["41"] = "TERM001",
                    ["49"] = "840"
                }
            };

            var response = await _client.PostAsJsonAsync("/api/iso/forward", req);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            Assert.True(
                content.Contains("PaymentAuthResponse", StringComparison.OrdinalIgnoreCase)
                || content.Contains("Success", StringComparison.OrdinalIgnoreCase)
                || content.Contains("Forwarded", StringComparison.OrdinalIgnoreCase),
                $"Unexpected forward response: {content}"
            );
        }
    }
}
