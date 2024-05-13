using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace IsoBridge.Adapters.Forwarding
{
    public sealed class RestForwarder : IForwarder
    {
        private readonly IHttpClientFactory _factory;
        private readonly IOptionsMonitor<ForwardingOptions> _opts;
        private readonly ILogger<RestForwarder>? _logger;

        public RestForwarder(IHttpClientFactory factory, IOptionsMonitor<ForwardingOptions> opts, ILogger<RestForwarder>? logger = null)
        {
            _factory = factory;
            _opts = opts;
            _logger = logger;
        }

        public async Task<ForwardResult> ForwardAsync(string routeKey, byte[] isoBytes, JsonObject mappedJson, CancellationToken ct = default)
        {
            if (!_opts.CurrentValue.Routes.TryGetValue(routeKey, out var route))
                throw new InvalidOperationException($"Route '{routeKey}' not found.");

            if (!string.Equals(route.Type, "rest", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException($"Route '{routeKey}' has unsupported type '{route.Type}'.");

            var client = _factory.CreateClient("IsoBridge.Forwarder");
            if (!Uri.TryCreate(route.BaseAddress, UriKind.Absolute, out var baseUri))
                throw new InvalidOperationException($"Invalid BaseAddress for route '{routeKey}': {route.BaseAddress}");
            client.BaseAddress = baseUri;

            var relative = route.Path.StartsWith("/") ? route.Path : "/" + route.Path;
            using var req = new HttpRequestMessage(new HttpMethod(route.Method), relative);

            if (route.SendRaw)
            {
                req.Content = new ByteArrayContent(isoBytes);
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            }
            else
            {
                var json = JsonSerializer.Serialize(mappedJson);
                req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            if (route.Headers is not null)
            {
                foreach (var kv in route.Headers)
                    req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
            }

            var rsp = await client.SendAsync(req, ct);
            var body = await rsp.Content.ReadAsStringAsync(ct);
            var headers = rsp.Headers.Concat(rsp.Content.Headers)
                .GroupBy(h => h.Key)
                .ToDictionary(g => g.Key, g => g.SelectMany(x => x.Value));

            _logger?.LogInformation("Forwarded to route '{RouteKey}' -> {Status}", routeKey, (int)rsp.StatusCode);
            return new ForwardResult((int)rsp.StatusCode, body, headers);
        }
    }
}