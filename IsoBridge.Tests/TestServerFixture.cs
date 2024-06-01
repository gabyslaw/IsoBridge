using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IsoBridge.Tests
{
    public class TestServerFixture : IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        public HttpClient Client { get; }

        public TestServerFixture()
        {
            _factory = new WebApplicationFactory<Program>();
            Client = _factory.CreateClient();
        }

        public void Dispose()
        {
            Client.Dispose();
            _factory.Dispose();
        }
    }
}