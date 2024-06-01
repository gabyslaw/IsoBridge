using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using IsoBridge.Adapters.Forwarding;
using IsoBridge.Core.ISO;
using IsoBridge.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IsoBridge.Web.Services
{
    public sealed class ForwardingService
    {
        private readonly IIsoParser _parser;
        private readonly IForwarder _forwarder;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ForwardingService> _logger;

        public ForwardingService(
            IIsoParser parser,
            IForwarder forwarder,
            IWebHostEnvironment env,
            ILogger<ForwardingService> logger)
        {
            _parser = parser;
            _forwarder = forwarder;
            _env = env;
            _logger = logger;
        }

        public (byte[] isoBytes, JsonObject json, bool success, string? error) BuildIsoAndMapJson(
            string mode,
            string encoding,
            string? payload,
            string? mti,
            Dictionary<int, string>? fields)
        {
            try
            {
                byte[] isoBytes;
                IsoMessage isoMsg;

                if (string.Equals(mode, "json", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(mti) || fields is null)
                        return (Array.Empty<byte>(), new JsonObject(), false, "Missing MTI or fields.");

                    isoMsg = new IsoMessage(mti, fields);
                    isoBytes = _parser.Build(isoMsg);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(payload))
                        return (Array.Empty<byte>(), new JsonObject(), false, "Missing payload.");

                    isoBytes = (encoding?.ToLowerInvariant()) switch
                    {
                        "base64" => Convert.FromBase64String(payload),
                        _ => Convert.FromHexString(payload)
                    };

                    var parsed = _parser.Parse(isoBytes);
                    if (!parsed.Success || parsed.Message is null)
                        return (Array.Empty<byte>(), new JsonObject(), false, parsed.Error ?? "Parse failed.");

                    isoMsg = parsed.Message;
                }

                var fieldJson = new JsonObject();
                foreach (var f in isoMsg.Fields)
                    fieldJson[f.Key.ToString()] = f.Value;

                var json = new JsonObject
                {
                    ["mti"] = isoMsg.Mti,
                    ["fields"] = fieldJson
                };

                return (isoBytes, json, true, null);
            }
            catch (Exception ex)
            {
                return (Array.Empty<byte>(), new JsonObject(), false, ex.Message);
            }
        }

        public Task<ForwardResult> ForwardAsync(string routeKey, byte[] isoBytes, JsonObject mappedJson, CancellationToken ct = default)
        {
            if (_env.IsEnvironment("Testing"))
            {
                _logger.LogInformation("Forwarding stubbed in Testing environment for route {RouteKey}.", routeKey);

                var jsonBody = new JsonObject
                {
                    ["status"] = "stubbed",
                    ["message"] = "Forwarded (test environment stub)",
                    ["route"] = routeKey,
                    ["timestamp"] = DateTime.UtcNow.ToString("O")
                };

                var stub = new ForwardResult(
                    200, // statusCode
                    jsonBody.ToJsonString(), // body as string
                    new Dictionary<string, IEnumerable<string>>() // empty headers
                );

                return Task.FromResult(stub);
            }

            return _forwarder.ForwardAsync(routeKey, isoBytes, mappedJson, ct);
        }
    }
}
