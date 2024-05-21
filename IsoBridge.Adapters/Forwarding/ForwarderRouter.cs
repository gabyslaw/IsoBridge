using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace IsoBridge.Adapters.Forwarding
{
    /// <summary>
    /// IForwarder that routes to REST or SOAP implementations based on route.Type
    /// </summary>
    public sealed class ForwarderRouter : IForwarder
    {
        private readonly IOptionsMonitor<ForwardingOptions> _opts;
        private readonly RestForwarder _rest;
        private readonly SoapForwarder _soap;

        public ForwarderRouter(IOptionsMonitor<ForwardingOptions> opts, RestForwarder rest, SoapForwarder soap)
        {
            _opts = opts;
            _rest = rest;
            _soap = soap;
        }

        public async Task<ForwardResult> ForwardAsync(string routeKey, byte[] isoBytes, JsonObject mappedJson, CancellationToken ct = default)
        {
            if (!_opts.CurrentValue.Routes.TryGetValue(routeKey, out var route))
                throw new InvalidOperationException($"Route '{routeKey}' not found.");

            return route.Type.ToLowerInvariant() switch
            {
                "rest" => await _rest.ForwardAsync(routeKey, isoBytes, mappedJson, ct),
                "soap" => await _soap.ForwardAsync(routeKey, mappedJson, ct),
                _ => throw new NotSupportedException($"Route type '{route.Type}' not supported.")
            };
        }
    }
}